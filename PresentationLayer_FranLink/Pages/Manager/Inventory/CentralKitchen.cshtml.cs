using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessLogicLayer_FranLink.DTOs;
using BusinessLogicLayer_FranLink.Services;
using DataAccessLayer_FranLink.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PresentationLayer_FranLink.Pages.Manager.Inventory
{
    public class CentralKitchenModel : PageModel
    {
        private readonly IInventoryService _inventoryService;
        private readonly ICentralKitchenService _centralKitchenService;

        public CentralKitchenModel(IInventoryService inventoryService, ICentralKitchenService centralKitchenService)
        {
            _inventoryService = inventoryService;
            _centralKitchenService = centralKitchenService;
        }

        public IList<DataAccessLayer_FranLink.Models.CentralKitchen> CentralKitchens { get; set; } = new List<DataAccessLayer_FranLink.Models.CentralKitchen>();
        public IList<DataAccessLayer_FranLink.Models.Inventory> Inventory { get; set; } = new List<DataAccessLayer_FranLink.Models.Inventory>();
        public int? SelectedKitchenId { get; set; }
        public CentralKitchenSummary? KitchenSummary { get; set; }

        public async Task<IActionResult> OnGetAsync(int? kitchenId)
        {
            // Check if user is Manager or Admin
            var role = HttpContext.Session.GetString("Role");
            if (role != "Manager" && role != "Admin")
            {
                return RedirectToPage("/Login");
            }

            CentralKitchens = await _centralKitchenService.GetAllCentralKitchensAsync();
            SelectedKitchenId = kitchenId;

            if (kitchenId.HasValue)
            {
                Inventory = await _inventoryService.GetCentralKitchenInventoryAsync(kitchenId.Value);
                KitchenSummary = await _centralKitchenService.GetCentralKitchenSummaryAsync(kitchenId.Value);
            }
            else
            {
                Inventory = await _inventoryService.GetAllCentralKitchenInventoryAsync();
            }

            return Page();
        }
    }
}
