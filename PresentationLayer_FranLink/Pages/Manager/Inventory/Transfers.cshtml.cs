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
    public class TransfersModel : PageModel
    {
        private readonly IInventoryService _inventoryService;
        private readonly ICentralKitchenService _centralKitchenService;
        private readonly FranLinkContext _context;

        public TransfersModel(IInventoryService inventoryService, ICentralKitchenService centralKitchenService, FranLinkContext context)
        {
            _inventoryService = inventoryService;
            _centralKitchenService = centralKitchenService;
            _context = context;
        }

        public string? StatusFilter { get; set; }
        public IList<InventoryTransfer> Transfers { get; set; } = new List<InventoryTransfer>();
        public IList<InventoryTransfer> AllTransfers { get; set; } = new List<InventoryTransfer>();
        public IList<CentralKitchen> CentralKitchens { get; set; } = new List<CentralKitchen>();
        public IList<FranchiseStore> Stores { get; set; } = new List<FranchiseStore>();
        public IList<Product> Products { get; set; } = new List<Product>();
        
        public string? Message { get; set; }
        public bool IsSuccess { get; set; }

        public async Task<IActionResult> OnGetAsync(string? status)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Manager" && role != "CentralKitchenStaff" && role != "Central Kitchen Staff")
            {
                return RedirectToPage("/Login");
            }

            StatusFilter = status;
            await LoadDataAsync();

            if (!string.IsNullOrEmpty(status))
            {
                Transfers = await _inventoryService.GetTransfersByStatusAsync(status);
            }
            else
            {
                Transfers = await _inventoryService.GetTransferHistoryAsync();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostCreateAsync(int? fromCentralKitchenId, int? fromStoreId, 
            int? toCentralKitchenId, int? toStoreId, int productId, int quantity, string? notes)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Manager" && role != "CentralKitchenStaff" && role != "Central Kitchen Staff")
            {
                return RedirectToPage("/Login");
            }

            // Get current user ID from session
            var userIdStr = HttpContext.Session.GetString("UserId");
            Guid userId = Guid.Empty;
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out userId))
            {
                // Fallback: get first user for demo
                var user = await _context.Users.FirstOrDefaultAsync();
                userId = user?.UserId ?? Guid.Empty;
            }

            var transfer = new InventoryTransfer
            {
                FromCentralKitchenId = fromCentralKitchenId,
                FromStoreId = fromStoreId,
                ToCentralKitchenId = toCentralKitchenId,
                ToStoreId = toStoreId,
                ProductId = productId,
                Quantity = quantity,
                Notes = notes,
                RequestedByUserId = userId
            };

            await _inventoryService.RequestTransferAsync(transfer);

            Message = "Transfer request created successfully!";
            IsSuccess = true;

            await LoadDataAsync();
            Transfers = await _inventoryService.GetPendingTransfersAsync();
            StatusFilter = "Pending";

            return Page();
        }

        public async Task<IActionResult> OnPostApproveAsync(int transferId)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Manager" && role != "CentralKitchenStaff" && role != "Central Kitchen Staff")
            {
                return RedirectToPage("/Login");
            }

            var userIdStr = HttpContext.Session.GetString("UserId");
            Guid.TryParse(userIdStr, out Guid userId);

            var result = await _inventoryService.ApproveTransferAsync(transferId, userId);
            
            if (result != null)
            {
                Message = $"Transfer #{transferId} approved successfully!";
                IsSuccess = true;
            }
            else
            {
                Message = $"Failed to approve transfer #{transferId}.";
                IsSuccess = false;
            }

            await LoadDataAsync();
            Transfers = await _inventoryService.GetPendingTransfersAsync();
            StatusFilter = "Pending";

            return Page();
        }

        public async Task<IActionResult> OnPostRejectAsync(int transferId)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Manager" && role != "CentralKitchenStaff" && role != "Central Kitchen Staff")
            {
                return RedirectToPage("/Login");
            }

            var userIdStr = HttpContext.Session.GetString("UserId");
            Guid.TryParse(userIdStr, out Guid userId);

            var result = await _inventoryService.RejectTransferAsync(transferId, userId);
            
            if (result != null)
            {
                Message = $"Transfer #{transferId} rejected.";
                IsSuccess = true;
            }
            else
            {
                Message = $"Failed to reject transfer #{transferId}.";
                IsSuccess = false;
            }

            await LoadDataAsync();
            Transfers = await _inventoryService.GetPendingTransfersAsync();
            StatusFilter = "Pending";

            return Page();
        }

        public async Task<IActionResult> OnPostCompleteAsync(int transferId)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Manager" && role != "Admin" && role != "CentralKitchenStaff")
            {
                return RedirectToPage("/Login");
            }

            var result = await _inventoryService.CompleteTransferAsync(transferId);
            
            if (result != null)
            {
                Message = $"Transfer #{transferId} completed! Inventory has been updated.";
                IsSuccess = true;
            }
            else
            {
                Message = $"Failed to complete transfer #{transferId}. Please check inventory availability.";
                IsSuccess = false;
            }

            await LoadDataAsync();
            Transfers = await _inventoryService.GetTransfersByStatusAsync("Approved");
            StatusFilter = "Approved";

            return Page();
        }

        private async Task LoadDataAsync()
        {
            CentralKitchens = await _centralKitchenService.GetAllCentralKitchensAsync();
            Stores = await _context.FranchiseStores.OrderBy(s => s.Name).ToListAsync();
            Products = await _context.Products.OrderBy(p => p.Name).ToListAsync();
            AllTransfers = await _inventoryService.GetTransferHistoryAsync();
        }
    }
}
