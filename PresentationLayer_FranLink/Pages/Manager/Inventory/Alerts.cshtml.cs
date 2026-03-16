using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessLogicLayer_FranLink.Services;
using DataAccessLayer_FranLink.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PresentationLayer_FranLink.Pages.Manager.Inventory
{
    public class AlertsModel : PageModel
    {
        private readonly IInventoryService _inventoryService;

        public AlertsModel(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        public string AlertType { get; set; } = "lowstock";
        public IList<DataAccessLayer_FranLink.Models.Inventory> LowStockItems { get; set; } = new List<DataAccessLayer_FranLink.Models.Inventory>();
        public IList<DataAccessLayer_FranLink.Models.Inventory> OverstockItems { get; set; } = new List<DataAccessLayer_FranLink.Models.Inventory>();
        public IList<DataAccessLayer_FranLink.Models.Inventory> ExpiringItems { get; set; } = new List<DataAccessLayer_FranLink.Models.Inventory>();
        public IList<DataAccessLayer_FranLink.Models.Inventory> ExpiredItems { get; set; } = new List<DataAccessLayer_FranLink.Models.Inventory>();

        public async Task<IActionResult> OnGetAsync(string? type)
        {
            // Check if user is Manager or Admin
            var role = HttpContext.Session.GetString("Role");
            if (role != "Manager" && role != "CentralKitchenStaff" && role != "Central Kitchen Staff")
            {
                return RedirectToPage("/Login");
            }

            AlertType = type ?? "lowstock";

            // Load all alert counts for tabs
            LowStockItems = await _inventoryService.GetLowStockItemsAsync();
            OverstockItems = await _inventoryService.GetOverstockItemsAsync();
            ExpiringItems = await _inventoryService.GetExpiringItemsAsync(7);
            ExpiredItems = await _inventoryService.GetExpiredItemsAsync();

            return Page();
        }
    }
}
