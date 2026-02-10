using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessLogicLayer_FranLink.Services;
using DataAccessLayer_FranLink.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PresentationLayer_FranLink.Pages.Inventory
{
    public class IndexModel : PageModel
    {
        private readonly IInventoryService _inventoryService;

        public IndexModel(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        public IList<DataAccessLayer_FranLink.Models.Inventory> Inventory { get; set; } = new List<DataAccessLayer_FranLink.Models.Inventory>();

        public async Task OnGetAsync()
        {
            // Hardcoded Store ID for demonstration.
            int storeId = 1; 
            Inventory = await _inventoryService.GetInventoryByStoreIdAsync(storeId);
        }
    }
}
