using BusinessLogicLayer_FranLink.DTOs;
using BusinessLogicLayer_FranLink.Services;
using DataAccessLayer_FranLink.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PresentationLayer_FranLink.Pages.Production.Batch
{
    public class CompleteModel : PageModel
    {
        private readonly IProductionService _productionService;

        public CompleteModel(IProductionService productionService)
        {
            _productionService = productionService;
        }

        public ProductionRecord Production { get; set; } = null!;

        [BindProperty]
        public CompleteProductionDto Input { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public string DashboardUrl { get; set; } = "Index";

        public async Task<IActionResult> OnGetAsync()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Manager" && role != "CentralKitchenStaff" && role != "Central Kitchen Staff")
            {
                return RedirectToPage("/Login");
            }

            DashboardUrl = GetDashboardPage();

            var production = await _productionService.GetByIdAsync(Id);
            if (production == null)
            {
                return NotFound();
            }

            if (production.Status != "InProgress")
            {
                TempData["Error"] = "Cannot complete a production that is not in progress.";
                return RedirectToPage(DashboardUrl);
            }

            Production = production;
            Input.ActualQuantity = production.PlannedQuantity;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Manager" && role != "CentralKitchenStaff" && role != "Central Kitchen Staff")
            {
                return RedirectToPage("/Login");
            }

            var result = await _productionService.CompleteProductionAsync(Id, Input);
            if (result == null)
            {
                TempData["Error"] = "Failed to complete production.";
                return RedirectToPage(GetDashboardPage());
            }

            TempData["Success"] = $"Production #{Id} completed successfully! Produced {Input.ActualQuantity} units.";
            return RedirectToPage(GetDashboardPage());
        }

        private string GetDashboardPage()
        {
            var role = HttpContext.Session.GetString("Role") ?? "";
            if (role == "CentralKitchenStaff" || role == "Central Kitchen Staff")
            {
                return "/CentralKitchenStaff/Index";
            }
            return "/Manager/Production/Index";
        }
    }
}
