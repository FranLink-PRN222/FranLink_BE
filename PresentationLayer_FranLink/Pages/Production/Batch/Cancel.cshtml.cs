using BusinessLogicLayer_FranLink.Services;
using DataAccessLayer_FranLink.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PresentationLayer_FranLink.Pages.Production.Batch
{
    public class CancelModel : PageModel
    {
        private readonly IProductionService _productionService;

        public CancelModel(IProductionService productionService)
        {
            _productionService = productionService;
        }

        public ProductionRecord Production { get; set; } = null!;

        [BindProperty]
        public string Reason { get; set; } = string.Empty;

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
                TempData["Error"] = "Cannot cancel a production that is not in progress.";
                return RedirectToPage(DashboardUrl);
            }

            Production = production;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Manager" && role != "CentralKitchenStaff" && role != "Central Kitchen Staff")
            {
                return RedirectToPage("/Login");
            }

            if (string.IsNullOrWhiteSpace(Reason))
            {
                ModelState.AddModelError("Reason", "Please provide a reason for cancellation.");
                var production = await _productionService.GetByIdAsync(Id);
                if (production != null) Production = production;
                DashboardUrl = GetDashboardPage();
                return Page();
            }

            var result = await _productionService.CancelProductionAsync(Id, Reason);
            if (result == null)
            {
                TempData["Error"] = "Failed to cancel production.";
                return RedirectToPage(GetDashboardPage());
            }

            TempData["Success"] = $"Production #{Id} has been cancelled.";
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
