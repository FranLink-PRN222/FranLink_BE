using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessLogicLayer_FranLink.Services;
using DataAccessLayer_FranLink.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PresentationLayer_FranLink.Pages.Inventory
{
    public class IndexModel : PageModel
    {
        private readonly IInventoryService _inventoryService;
        private readonly FranLinkContext _context;

        public IndexModel(IInventoryService inventoryService, FranLinkContext context)
        {
            _inventoryService = inventoryService;
            _context = context;
        }

        public IList<DataAccessLayer_FranLink.Models.Inventory> Inventory { get; set; } = new List<DataAccessLayer_FranLink.Models.Inventory>();

        public async Task OnGetAsync()
        {
            // Default fallback store
            int storeId = 1;

            var userIdStr = HttpContext.Session.GetString("UserId");
            if (!string.IsNullOrEmpty(userIdStr) && Guid.TryParse(userIdStr, out var userId))
            {
                var user = await _context.Users.FindAsync(userId);
                if (user?.FranchiseStoreId != null)
                {
                    storeId = user.FranchiseStoreId.Value;
                }
            }

            Inventory = await _inventoryService.GetInventoryByStoreIdAsync(storeId);
        }
    }
}
