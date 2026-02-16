using BusinessLogicLayer_FranLink.DTOs;
using BusinessLogicLayer_FranLink.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PresentationLayer_FranLink.Pages.Manager.Production
{
    public class StartModel : PageModel
    {
        private readonly IProductionService _productionService;
        private readonly IRecipeService _recipeService;

        public StartModel(IProductionService productionService, IRecipeService recipeService)
        {
            _productionService = productionService;
            _recipeService = recipeService;
        }

        [BindProperty]
        public StartProductionDto Input { get; set; } = new();

        public List<SelectListItem> CentralKitchens { get; set; } = new();
        public List<SelectListItem> Recipes { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Manager" && role != "Admin")
            {
                return RedirectToPage("/Login");
            }

            await LoadSelectListsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Manager" && role != "Admin")
            {
                return RedirectToPage("/Login");
            }

            if (!ModelState.IsValid)
            {
                await LoadSelectListsAsync();
                return Page();
            }

            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            {
                return RedirectToPage("/Login");
            }

            // Check ingredient availability
            var availability = await _recipeService.GetIngredientsAvailabilityDetailAsync(
                Input.RecipeId, Input.PlannedQuantity, Input.CentralKitchenId);

            var shortage = availability.Where(a => !a.IsAvailable).ToList();
            if (shortage.Any())
            {
                foreach (var item in shortage)
                {
                    ModelState.AddModelError("", $"Insufficient {item.ProductName}: need {item.RequiredQuantity}, have {item.AvailableQuantity} (shortage: {item.ShortageQuantity})");
                }
                await LoadSelectListsAsync();
                return Page();
            }

            await _productionService.StartProductionAsync(Input, userId);
            TempData["Success"] = "Production started successfully!";
            return RedirectToPage("Index");
        }

        private async Task LoadSelectListsAsync()
        {
            var kitchens = await _productionService.GetAllCentralKitchensAsync();
            CentralKitchens = kitchens.Select(ck => new SelectListItem
            {
                Value = ck.Id.ToString(),
                Text = ck.Name
            }).ToList();

            var recipes = await _productionService.GetActiveRecipesAsync();
            Recipes = recipes.Select(r => new SelectListItem
            {
                Value = r.RecipeId.ToString(),
                Text = $"{r.Name} ({r.Product.Name})"
            }).ToList();
        }
    }
}
