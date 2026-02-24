using BusinessLogicLayer_FranLink.DTOs;
using BusinessLogicLayer_FranLink.Services;
using DataAccessLayer_FranLink.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PresentationLayer_FranLink.Pages.Manager.Production
{
    public class IndexModel : PageModel
    {
        private readonly IProductionService _productionService;

        public IndexModel(IProductionService productionService)
        {
            _productionService = productionService;
        }

        public List<ProductionRecord> InProgressProductions { get; set; } = new();
        public ProductionSummary TodaySummary { get; set; } = new();
        public List<ProductionByProduct> TopProducts { get; set; } = new();
        public List<SelectListItem> CentralKitchens { get; set; } = new();
        public List<SelectListItem> Recipes { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int? CentralKitchenId { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Manager" && role != "Admin" && role != "CentralKitchenStaff")
            {
                return RedirectToPage("/Login");
            }

            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            InProgressProductions = await _productionService.GetInProgressProductionsAsync(CentralKitchenId);
            TodaySummary = await _productionService.GetProductionSummaryAsync(today, tomorrow, CentralKitchenId);
            TopProducts = await _productionService.GetTopProductsAsync(today.AddDays(-7), tomorrow, 5);

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

            return Page();
        }
    }
}
