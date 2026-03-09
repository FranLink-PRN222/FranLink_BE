using BusinessLogicLayer_FranLink.DTOs;
using BusinessLogicLayer_FranLink.Services;
using DataAccessLayer_FranLink.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PresentationLayer_FranLink.Pages.Manager.Recipes
{
    public class DetailsModel : PageModel
    {
        private readonly IRecipeService _recipeService;
        private readonly ICentralKitchenService _centralKitchenService;

        public DetailsModel(IRecipeService recipeService, ICentralKitchenService centralKitchenService)
        {
            _recipeService = recipeService;
            _centralKitchenService = centralKitchenService;
        }

        public Recipe Recipe { get; set; } = null!;
        public List<CentralKitchen> CentralKitchens { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int? SelectedCentralKitchenId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int CalculateQuantity { get; set; } = 10;

        public List<IngredientAvailability> IngredientsAvailability { get; set; } = new();
        public bool CanProduce { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Manager")
            {
                return RedirectToPage("/Login");
            }

            var recipe = await _recipeService.GetRecipeByIdAsync(id);
            if (recipe == null)
            {
                return NotFound();
            }

            Recipe = recipe;
            CentralKitchens = await _centralKitchenService.GetAllCentralKitchensAsync();

            // If a central kitchen is selected, calculate availability
            if (SelectedCentralKitchenId.HasValue && CalculateQuantity > 0)
            {
                IngredientsAvailability = await _recipeService.GetIngredientsAvailabilityDetailAsync(
                    id, CalculateQuantity, SelectedCentralKitchenId.Value);
                CanProduce = IngredientsAvailability.All(i => i.IsAvailable);
            }

            return Page();
        }
    }
}
