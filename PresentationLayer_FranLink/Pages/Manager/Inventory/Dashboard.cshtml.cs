using System.Threading.Tasks;
using BusinessLogicLayer_FranLink.DTOs;
using BusinessLogicLayer_FranLink.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PresentationLayer_FranLink.Pages.Manager.Inventory
{
    public class DashboardModel : PageModel
    {
        private readonly IInventoryService _inventoryService;

        public DashboardModel(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        public InventoryDashboardSummary Summary { get; set; } = new InventoryDashboardSummary();

        public async Task<IActionResult> OnGetAsync()
        {
            // Check if user is Manager or Admin
            var role = HttpContext.Session.GetString("Role");
            if (role != "Manager" && role != "Admin")
            {
                return RedirectToPage("/Login");
            }

            Summary = await _inventoryService.GetDashboardSummaryAsync();
            return Page();
        }
    }
}
