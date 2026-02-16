using BusinessLogicLayer_FranLink.DTOs;
using BusinessLogicLayer_FranLink.Services;
using DataAccessLayer_FranLink.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace PresentationLayer_FranLink.Pages.Manager.Recipes
{
    public class EditModel : PageModel
    {
        private readonly IRecipeService _recipeService;
        private readonly IProductService _productService;

        public EditModel(IRecipeService recipeService, IProductService productService)
        {
            _recipeService = recipeService;
            _productService = productService;
        }

        [BindProperty]
        public RecipeInput Input { get; set; } = new();

        public SelectList ProductList { get; set; } = null!;
        public SelectList IngredientList { get; set; } = null!;

        public class RecipeInput
        {
            public int RecipeId { get; set; }

            [Required(ErrorMessage = "Recipe name is required")]
            [StringLength(200)]
            public string Name { get; set; } = string.Empty;

            [Required(ErrorMessage = "Output product is required")]
            public int ProductId { get; set; }

            [StringLength(2000)]
            public string? Instructions { get; set; }

            [Required]
            [Range(1, int.MaxValue, ErrorMessage = "Output quantity must be at least 1")]
            public int OutputQuantity { get; set; } = 1;

            [Range(0, double.MaxValue, ErrorMessage = "Estimated time must be positive")]
            public decimal EstimatedTime { get; set; }

            public bool IsActive { get; set; }

            public List<RecipeItemInput> Items { get; set; } = new();
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Manager" && role != "Admin")
            {
                return RedirectToPage("/Login");
            }

            var recipe = await _recipeService.GetRecipeByIdAsync(id);
            if (recipe == null)
            {
                return NotFound();
            }

            Input = new RecipeInput
            {
                RecipeId = recipe.RecipeId,
                Name = recipe.Name,
                ProductId = recipe.ProductId,
                Instructions = recipe.Instructions,
                OutputQuantity = recipe.OutputQuantity,
                EstimatedTime = recipe.EstimatedTime,
                IsActive = recipe.IsActive,
                Items = recipe.RecipeItems.Select(ri => new RecipeItemInput
                {
                    RecipeItemId = ri.RecipeItemId,
                    IngredientProductId = ri.IngredientProductId,
                    Quantity = ri.Quantity,
                    Unit = ri.Unit,
                    Notes = ri.Notes
                }).ToList()
            };

            if (!Input.Items.Any())
            {
                Input.Items.Add(new RecipeItemInput());
            }

            await LoadSelectListsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Remove empty items
            Input.Items = Input.Items
                .Where(i => i.IngredientProductId > 0 && i.Quantity > 0)
                .ToList();

            if (!ModelState.IsValid)
            {
                await LoadSelectListsAsync();
                if (!Input.Items.Any())
                {
                    Input.Items.Add(new RecipeItemInput());
                }
                return Page();
            }

            try
            {
                // Update recipe
                var recipe = new Recipe
                {
                    RecipeId = Input.RecipeId,
                    Name = Input.Name,
                    ProductId = Input.ProductId,
                    Instructions = Input.Instructions,
                    OutputQuantity = Input.OutputQuantity,
                    EstimatedTime = Input.EstimatedTime,
                    IsActive = Input.IsActive
                };

                await _recipeService.UpdateRecipeAsync(recipe);

                // Get existing items
                var existingItems = await _recipeService.GetRecipeItemsAsync(Input.RecipeId);
                var existingItemIds = existingItems.Select(i => i.RecipeItemId).ToList();
                var inputItemIds = Input.Items.Where(i => i.RecipeItemId > 0).Select(i => i.RecipeItemId).ToList();

                // Delete removed items
                foreach (var existingId in existingItemIds)
                {
                    if (!inputItemIds.Contains(existingId))
                    {
                        await _recipeService.RemoveRecipeItemAsync(existingId);
                    }
                }

                // Update or add items
                foreach (var item in Input.Items)
                {
                    if (item.RecipeItemId > 0)
                    {
                        // Update existing
                        var recipeItem = new RecipeItem
                        {
                            RecipeItemId = item.RecipeItemId,
                            Quantity = item.Quantity,
                            Unit = item.Unit,
                            Notes = item.Notes
                        };
                        await _recipeService.UpdateRecipeItemAsync(recipeItem);
                    }
                    else
                    {
                        // Add new
                        var recipeItem = new RecipeItem
                        {
                            RecipeId = Input.RecipeId,
                            IngredientProductId = item.IngredientProductId,
                            Quantity = item.Quantity,
                            Unit = item.Unit,
                            Notes = item.Notes
                        };
                        await _recipeService.AddRecipeItemAsync(recipeItem);
                    }
                }

                TempData["SuccessMessage"] = $"Recipe '{recipe.Name}' updated successfully.";
                return RedirectToPage("Index");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await LoadSelectListsAsync();
                return Page();
            }
        }

        private async Task LoadSelectListsAsync()
        {
            var products = await _productService.GetActiveProductsAsync();
            ProductList = new SelectList(products, "Id", "Name");
            IngredientList = new SelectList(products, "Id", "Name");
        }
    }
}
