using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessLogicLayer_FranLink.DTOs;
using DataAccessLayer_FranLink.Models;

namespace BusinessLogicLayer_FranLink.Services
{
    public interface IInventoryService
    {
        // Basic inventory queries
        Task<List<Inventory>> GetInventoryByStoreIdAsync(int storeId);
        Task<Inventory?> GetInventoryByIdAsync(Guid inventoryId);
         
        // Central Kitchen inventory
        Task<List<Inventory>> GetCentralKitchenInventoryAsync(int centralKitchenId);
        Task<List<Inventory>> GetAllCentralKitchenInventoryAsync();
        
        // Store inventory
        Task<List<Inventory>> GetAllStoresInventoryAsync();
        
        // Inventory alerts
        Task<List<Inventory>> GetLowStockItemsAsync(int? storeId = null, int? centralKitchenId = null);
        Task<List<Inventory>> GetOverstockItemsAsync(int? storeId = null, int? centralKitchenId = null);
        Task<List<Inventory>> GetExpiringItemsAsync(int daysUntilExpiry = 7, int? storeId = null, int? centralKitchenId = null);
        Task<List<Inventory>> GetExpiredItemsAsync(int? storeId = null, int? centralKitchenId = null);
        
        // Inventory management
        Task<Inventory> AddInventoryAsync(Inventory inventory);
        Task<Inventory> UpdateInventoryAsync(Inventory inventory);
        Task<bool> AdjustQuantityAsync(Guid inventoryId, int quantityChange, string reason);
        
        // Transfer management
        Task<List<InventoryTransfer>> GetPendingTransfersAsync();
        Task<List<InventoryTransfer>> GetTransfersByStatusAsync(string status);
        Task<List<InventoryTransfer>> GetTransferHistoryAsync(int? storeId = null, int? centralKitchenId = null);
        Task<InventoryTransfer?> GetTransferByIdAsync(int transferId);
        Task<InventoryTransfer> RequestTransferAsync(InventoryTransfer transfer);
        Task<InventoryTransfer?> ApproveTransferAsync(int transferId, Guid approvedByUserId);
        Task<InventoryTransfer?> RejectTransferAsync(int transferId, Guid rejectedByUserId, string? reason = null);
        Task<InventoryTransfer?> CompleteTransferAsync(int transferId);
        
        // Disposal management
        Task<List<InventoryDisposal>> GetPendingDisposalsAsync();
        Task<List<InventoryDisposal>> GetDisposalsByStatusAsync(string status);
        Task<List<InventoryDisposal>> GetDisposalHistoryAsync(int? storeId = null, int? centralKitchenId = null);
        Task<InventoryDisposal?> GetDisposalByIdAsync(int disposalId);
        Task<InventoryDisposal> RequestDisposalAsync(InventoryDisposal disposal);
        Task<InventoryDisposal?> ApproveDisposalAsync(int disposalId, Guid approvedByUserId);
        Task<InventoryDisposal?> RejectDisposalAsync(int disposalId, Guid rejectedByUserId, string? reason = null);
        
        // Dashboard summary
        Task<InventoryDashboardSummary> GetDashboardSummaryAsync();
    }
}
