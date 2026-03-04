using BusinessLogicLayer_FranLink.DTOs;
using BusinessLogicLayer_FranLink.Services;
using DataAccessLayer_FranLink.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace PresentationLayer_FranLink.Pages.Manager.Recipes
{
    public class CreateModel : PageModel
    {
        private readonly IRecipeService _recipeService;
        private readonly IProductService _productService;

        public CreateModel(IRecipeService recipeService, IProductService productService)
        {
            _recipeService = recipeService;
            _productService = productService;
        }

        [BindProperty]
        public RecipeInput Input { get; set; } = new();

        public SelectList ProductList { get; set; } = null!;
        public SelectList IngredientList { get; set; } = null!;
        public int? PreselectedProductId { get; set; }

        public class RecipeInput
        {
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

            public List<RecipeItemInput> Items { get; set; } = new();
        }

        public async Task<IActionResult> OnGetAsync(int? productId)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Manager" && role != "Admin")
            {
                return RedirectToPage("/Login");
            }

            await LoadSelectListsAsync();

            if (productId.HasValue)
            {
                PreselectedProductId = productId;
                Input.ProductId = productId.Value;
                
                var product = await _productService.GetProductByIdAsync(productId.Value);
                if (product != null)
                {
                    Input.Name = $"Recipe for {product.Name}";
                }
            }

            // Add one empty ingredient row
            Input.Items.Add(new RecipeItemInput());

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
                var recipe = new Recipe
                {
                    Name = Input.Name,
                    ProductId = Input.ProductId,
                    Instructions = Input.Instructions,
                    OutputQuantity = Input.OutputQuantity,
                    EstimatedTime = Input.EstimatedTime
                };

                var createdRecipe = await _recipeService.CreateRecipeAsync(recipe);

                // Add recipe items
                foreach (var item in Input.Items)
                {
                    var recipeItem = new RecipeItem
                    {
                        RecipeId = createdRecipe.RecipeId,
                        IngredientProductId = item.IngredientProductId,
                        Quantity = item.Quantity,
                        Unit = item.Unit,
                        Notes = item.Notes
                    };
                    await _recipeService.AddRecipeItemAsync(recipeItem);
                }

                TempData["SuccessMessage"] = $"Recipe '{recipe.Name}' created successfully.";
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
