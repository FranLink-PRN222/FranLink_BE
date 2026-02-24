using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessLogicLayer_FranLink.Services;
using DataAccessLayer_FranLink.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace PresentationLayer_FranLink.Pages.Manager.Inventory
{
    public class DisposalsModel : PageModel
    {
        private readonly IInventoryService _inventoryService;
        private readonly FranLinkContext _context;

        public DisposalsModel(IInventoryService inventoryService, FranLinkContext context)
        {
            _inventoryService = inventoryService;
            _context = context;
        }

        public string? StatusFilter { get; set; }
        public IList<InventoryDisposal> Disposals { get; set; } = new List<InventoryDisposal>();
        public IList<InventoryDisposal> AllDisposals { get; set; } = new List<InventoryDisposal>();
        public IList<DataAccessLayer_FranLink.Models.Inventory> InventoryItems { get; set; } = new List<DataAccessLayer_FranLink.Models.Inventory>();
        
        public string? Message { get; set; }
        public bool IsSuccess { get; set; }

        public async Task<IActionResult> OnGetAsync(string? status)
        {
            // Check if user is Manager or Admin
            var role = HttpContext.Session.GetString("Role");
            if (role != "Manager" && role != "Admin" && role != "CentralKitchenStaff")
            {
                return RedirectToPage("/Login");
            }

            StatusFilter = status;
            await LoadDataAsync();

            if (!string.IsNullOrEmpty(status))
            {
                Disposals = await _inventoryService.GetDisposalsByStatusAsync(status);
            }
            else
            {
                Disposals = await _inventoryService.GetDisposalHistoryAsync();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostCreateAsync(Guid inventoryId, int quantity, string reason, string? notes)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Manager" && role != "Admin" && role != "CentralKitchenStaff")
            {
                return RedirectToPage("/Login");
            }

            // Get current user ID from session
            var userIdStr = HttpContext.Session.GetString("UserId");
            Guid userId = Guid.Empty;
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out userId))
            {
                // Fallback: get first user for demo
                userId = Guid.Empty;
            }

            var disposal = new InventoryDisposal
            {
                InventoryId = inventoryId,
                Quantity = quantity,
                Reason = reason,
                Notes = notes,
                DisposedByUserId = userId
            };

            await _inventoryService.RequestDisposalAsync(disposal);

            Message = "Disposal request created successfully!";
            IsSuccess = true;

            await LoadDataAsync();
            Disposals = await _inventoryService.GetPendingDisposalsAsync();
            StatusFilter = "Pending";

            return Page();
        }

        public async Task<IActionResult> OnPostApproveAsync(int disposalId)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Manager" && role != "Admin" && role != "CentralKitchenStaff")
            {
                return RedirectToPage("/Login");
            }

            var userIdStr = HttpContext.Session.GetString("UserId");
            Guid.TryParse(userIdStr, out Guid userId);

            var result = await _inventoryService.ApproveDisposalAsync(disposalId, userId);
            
            if (result != null)
            {
                Message = $"Disposal #{disposalId} approved! Inventory has been reduced by {result.Quantity} units.";
                IsSuccess = true;
            }
            else
            {
                Message = $"Failed to approve disposal #{disposalId}.";
                IsSuccess = false;
            }

            await LoadDataAsync();
            Disposals = await _inventoryService.GetPendingDisposalsAsync();
            StatusFilter = "Pending";

            return Page();
        }

        public async Task<IActionResult> OnPostRejectAsync(int disposalId)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Manager" && role != "Admin" && role != "CentralKitchenStaff")
            {
                return RedirectToPage("/Login");
            }

            var userIdStr = HttpContext.Session.GetString("UserId");
            Guid.TryParse(userIdStr, out Guid userId);

            var result = await _inventoryService.RejectDisposalAsync(disposalId, userId);
            
            if (result != null)
            {
                Message = $"Disposal #{disposalId} rejected.";
                IsSuccess = true;
            }
            else
            {
                Message = $"Failed to reject disposal #{disposalId}.";
                IsSuccess = false;
            }

            await LoadDataAsync();
            Disposals = await _inventoryService.GetPendingDisposalsAsync();
            StatusFilter = "Pending";

            return Page();
        }

        private async Task LoadDataAsync()
        {
            AllDisposals = await _inventoryService.GetDisposalHistoryAsync();
            
            // Load all inventory items for the create modal
            InventoryItems = await _context.Inventories
                .Include(i => i.Product)
                .Include(i => i.CentralKitchen)
                .Include(i => i.FranchiseStore)
                .Where(i => i.Quantity > 0)
                .OrderBy(i => i.Product.Name)
                .ToListAsync();
        }
    }
}
