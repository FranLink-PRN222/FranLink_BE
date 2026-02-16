using BusinessLogicLayer_FranLink.Services;
using DataAccessLayer_FranLink.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PresentationLayer_FranLink.Pages.Manager.Recipes
{
    public class IndexModel : PageModel
    {
        private readonly IRecipeService _recipeService;

        public IndexModel(IRecipeService recipeService)
        {
            _recipeService = recipeService;
        }

        public List<Recipe> Recipes { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public bool ShowInactive { get; set; } = false;

        public string? SuccessMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Check if user is Manager or Admin
            var role = HttpContext.Session.GetString("Role");
            if (role != "Manager" && role != "Admin")
            {
                return RedirectToPage("/Login");
            }

            Recipes = ShowInactive
                ? await _recipeService.GetAllRecipesAsync()
                : await _recipeService.GetActiveRecipesAsync();

            // Check for success message from TempData
            if (TempData["SuccessMessage"] != null)
            {
                SuccessMessage = TempData["SuccessMessage"]?.ToString();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var result = await _recipeService.DeleteRecipeAsync(id);
            if (result)
            {
                TempData["SuccessMessage"] = "Recipe deactivated successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to deactivate recipe.";
            }

            return RedirectToPage();
        }
    }
}
