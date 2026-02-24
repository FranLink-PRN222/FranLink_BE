using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessLogicLayer_FranLink.DTOs;
using DataAccessLayer_FranLink.Models;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogicLayer_FranLink.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly FranLinkContext _context;

        public InventoryService(FranLinkContext context)
        {
            _context = context;
        }

        #region Basic Inventory Queries

        public async Task<List<Inventory>> GetInventoryByStoreIdAsync(int storeId)
        {
            return await _context.Inventories
                .Include(i => i.Product)
                .Include(i => i.FranchiseStore)
                .Where(i => i.FranchiseStoreId == storeId)
                .ToListAsync();
        }

        public async Task<Inventory?> GetInventoryByIdAsync(Guid inventoryId)
        {
            return await _context.Inventories
                .Include(i => i.Product)
                .Include(i => i.FranchiseStore)
                .Include(i => i.CentralKitchen)
                .FirstOrDefaultAsync(i => i.InventoryId == inventoryId);
        }

        #endregion

        #region Central Kitchen Inventory

        public async Task<List<Inventory>> GetCentralKitchenInventoryAsync(int centralKitchenId)
        {
            return await _context.Inventories
                .Include(i => i.Product)
                .Include(i => i.CentralKitchen)
                .Where(i => i.CentralKitchenId == centralKitchenId)
                .OrderBy(i => i.Product.Name)
                .ToListAsync();
        }

        public async Task<List<Inventory>> GetAllCentralKitchenInventoryAsync()
        {
            return await _context.Inventories
                .Include(i => i.Product)
                .Include(i => i.CentralKitchen)
                .Where(i => i.CentralKitchenId != null)
                .OrderBy(i => i.CentralKitchen!.Name)
                .ThenBy(i => i.Product.Name)
                .ToListAsync();
        }

        #endregion

        #region Store Inventory

        public async Task<List<Inventory>> GetAllStoresInventoryAsync()
        {
            return await _context.Inventories
                .Include(i => i.Product)
                .Include(i => i.FranchiseStore)
                .Where(i => i.FranchiseStoreId != null)
                .OrderBy(i => i.FranchiseStore!.Name)
                .ThenBy(i => i.Product.Name)
                .ToListAsync();
        }

        #endregion

        #region Inventory Alerts

        public async Task<List<Inventory>> GetLowStockItemsAsync(int? storeId = null, int? centralKitchenId = null)
        {
            var query = _context.Inventories
                .Include(i => i.Product)
                .Include(i => i.FranchiseStore)
                .Include(i => i.CentralKitchen)
                .Where(i => i.Quantity <= i.MinThreshold);

            if (storeId.HasValue)
                query = query.Where(i => i.FranchiseStoreId == storeId);
            if (centralKitchenId.HasValue)
                query = query.Where(i => i.CentralKitchenId == centralKitchenId);

            return await query.OrderBy(i => i.Quantity).ToListAsync();
        }

        public async Task<List<Inventory>> GetOverstockItemsAsync(int? storeId = null, int? centralKitchenId = null)
        {
            var query = _context.Inventories
                .Include(i => i.Product)
                .Include(i => i.FranchiseStore)
                .Include(i => i.CentralKitchen)
                .Where(i => i.Quantity >= i.MaxThreshold);

            if (storeId.HasValue)
                query = query.Where(i => i.FranchiseStoreId == storeId);
            if (centralKitchenId.HasValue)
                query = query.Where(i => i.CentralKitchenId == centralKitchenId);

            return await query.OrderByDescending(i => i.Quantity).ToListAsync();
        }

        public async Task<List<Inventory>> GetExpiringItemsAsync(int daysUntilExpiry = 7, int? storeId = null, int? centralKitchenId = null)
        {
            var expiryThreshold = DateTime.UtcNow.AddDays(daysUntilExpiry);
            var now = DateTime.UtcNow;

            var query = _context.Inventories
                .Include(i => i.Product)
                .Include(i => i.FranchiseStore)
                .Include(i => i.CentralKitchen)
                .Where(i => i.ExpiryDate != null && i.ExpiryDate > now && i.ExpiryDate <= expiryThreshold);

            if (storeId.HasValue)
                query = query.Where(i => i.FranchiseStoreId == storeId);
            if (centralKitchenId.HasValue)
                query = query.Where(i => i.CentralKitchenId == centralKitchenId);

            return await query.OrderBy(i => i.ExpiryDate).ToListAsync();
        }

        public async Task<List<Inventory>> GetExpiredItemsAsync(int? storeId = null, int? centralKitchenId = null)
        {
            var now = DateTime.UtcNow;

            var query = _context.Inventories
                .Include(i => i.Product)
                .Include(i => i.FranchiseStore)
                .Include(i => i.CentralKitchen)
                .Where(i => i.ExpiryDate != null && i.ExpiryDate <= now);

            if (storeId.HasValue)
                query = query.Where(i => i.FranchiseStoreId == storeId);
            if (centralKitchenId.HasValue)
                query = query.Where(i => i.CentralKitchenId == centralKitchenId);

            return await query.OrderBy(i => i.ExpiryDate).ToListAsync();
        }

        #endregion

        #region Inventory Management

        public async Task<Inventory> AddInventoryAsync(Inventory inventory)
        {
            inventory.InventoryId = Guid.NewGuid();
            inventory.LastUpdated = DateTime.UtcNow;
            _context.Inventories.Add(inventory);
            await _context.SaveChangesAsync();
            return inventory;
        }

        public async Task<Inventory> UpdateInventoryAsync(Inventory inventory)
        {
            inventory.LastUpdated = DateTime.UtcNow;
            _context.Inventories.Update(inventory);
            await _context.SaveChangesAsync();
            return inventory;
        }

        public async Task<bool> AdjustQuantityAsync(Guid inventoryId, int quantityChange, string reason)
        {
            var inventory = await _context.Inventories.FindAsync(inventoryId);
            if (inventory == null) return false;

            inventory.Quantity += quantityChange;
            if (inventory.Quantity < 0) inventory.Quantity = 0;
            inventory.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Transfer Management

        public async Task<List<InventoryTransfer>> GetPendingTransfersAsync()
        {
            return await GetTransfersByStatusAsync("Pending");
        }

        public async Task<List<InventoryTransfer>> GetTransfersByStatusAsync(string status)
        {
            return await _context.InventoryTransfers
                .Include(t => t.Product)
                .Include(t => t.FromCentralKitchen)
                .Include(t => t.FromStore)
                .Include(t => t.ToCentralKitchen)
                .Include(t => t.ToStore)
                .Include(t => t.RequestedBy)
                .Include(t => t.ApprovedBy)
                .Where(t => t.Status == status)
                .OrderByDescending(t => t.RequestDate)
                .ToListAsync();
        }

        public async Task<List<InventoryTransfer>> GetTransferHistoryAsync(int? storeId = null, int? centralKitchenId = null)
        {
            var query = _context.InventoryTransfers
                .Include(t => t.Product)
                .Include(t => t.FromCentralKitchen)
                .Include(t => t.FromStore)
                .Include(t => t.ToCentralKitchen)
                .Include(t => t.ToStore)
                .Include(t => t.RequestedBy)
                .Include(t => t.ApprovedBy)
                .AsQueryable();

            if (storeId.HasValue)
            {
                query = query.Where(t => t.FromStoreId == storeId || t.ToStoreId == storeId);
            }
            if (centralKitchenId.HasValue)
            {
                query = query.Where(t => t.FromCentralKitchenId == centralKitchenId || t.ToCentralKitchenId == centralKitchenId);
            }

            return await query.OrderByDescending(t => t.RequestDate).ToListAsync();
        }

        public async Task<InventoryTransfer?> GetTransferByIdAsync(int transferId)
        {
            return await _context.InventoryTransfers
                .Include(t => t.Product)
                .Include(t => t.FromCentralKitchen)
                .Include(t => t.FromStore)
                .Include(t => t.ToCentralKitchen)
                .Include(t => t.ToStore)
                .Include(t => t.RequestedBy)
                .Include(t => t.ApprovedBy)
                .FirstOrDefaultAsync(t => t.Id == transferId);
        }

        public async Task<InventoryTransfer> RequestTransferAsync(InventoryTransfer transfer)
        {
            transfer.Status = "Pending";
            transfer.RequestDate = DateTime.UtcNow;
            _context.InventoryTransfers.Add(transfer);
            await _context.SaveChangesAsync();
            return transfer;
        }

        public async Task<InventoryTransfer?> ApproveTransferAsync(int transferId, Guid approvedByUserId)
        {
            var transfer = await _context.InventoryTransfers.FindAsync(transferId);
            if (transfer == null || transfer.Status != "Pending") return null;

            transfer.Status = "Approved";
            transfer.ApprovedByUserId = approvedByUserId;
            transfer.ApprovedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return transfer;
        }

        public async Task<InventoryTransfer?> RejectTransferAsync(int transferId, Guid rejectedByUserId, string? reason = null)
        {
            var transfer = await _context.InventoryTransfers.FindAsync(transferId);
            if (transfer == null || transfer.Status != "Pending") return null;

            transfer.Status = "Rejected";
            transfer.ApprovedByUserId = rejectedByUserId;
            transfer.ApprovedDate = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(reason))
            {
                transfer.Notes = (transfer.Notes ?? "") + $" [Rejected: {reason}]";
            }

            await _context.SaveChangesAsync();
            return transfer;
        }

        public async Task<InventoryTransfer?> CompleteTransferAsync(int transferId)
        {
            var transfer = await _context.InventoryTransfers
                .Include(t => t.Product)
                .FirstOrDefaultAsync(t => t.Id == transferId);

            if (transfer == null || transfer.Status != "Approved") return null;

            // Find source inventory
            Inventory? sourceInventory = null;
            if (transfer.FromCentralKitchenId.HasValue)
            {
                sourceInventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.CentralKitchenId == transfer.FromCentralKitchenId && i.ProductId == transfer.ProductId);
            }
            else if (transfer.FromStoreId.HasValue)
            {
                sourceInventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.FranchiseStoreId == transfer.FromStoreId && i.ProductId == transfer.ProductId);
            }

            if (sourceInventory == null || sourceInventory.Quantity < transfer.Quantity)
            {
                return null; // Insufficient inventory
            }

            // Find or create destination inventory
            Inventory? destInventory = null;
            if (transfer.ToCentralKitchenId.HasValue)
            {
                destInventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.CentralKitchenId == transfer.ToCentralKitchenId && i.ProductId == transfer.ProductId);
                
                if (destInventory == null)
                {
                    destInventory = new Inventory
                    {
                        InventoryId = Guid.NewGuid(),
                        CentralKitchenId = transfer.ToCentralKitchenId,
                        ProductId = transfer.ProductId,
                        Quantity = 0,
                        BatchNumber = sourceInventory.BatchNumber,
                        ExpiryDate = sourceInventory.ExpiryDate,
                        MinThreshold = sourceInventory.MinThreshold,
                        MaxThreshold = sourceInventory.MaxThreshold,
                        LastUpdated = DateTime.UtcNow
                    };
                    _context.Inventories.Add(destInventory);
                }
            }
            else if (transfer.ToStoreId.HasValue)
            {
                destInventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.FranchiseStoreId == transfer.ToStoreId && i.ProductId == transfer.ProductId);
                
                if (destInventory == null)
                {
                    destInventory = new Inventory
                    {
                        InventoryId = Guid.NewGuid(),
                        FranchiseStoreId = transfer.ToStoreId,
                        ProductId = transfer.ProductId,
                        Quantity = 0,
                        BatchNumber = sourceInventory.BatchNumber,
                        ExpiryDate = sourceInventory.ExpiryDate,
                        MinThreshold = sourceInventory.MinThreshold,
                        MaxThreshold = sourceInventory.MaxThreshold,
                        LastUpdated = DateTime.UtcNow
                    };
                    _context.Inventories.Add(destInventory);
                }
            }

            if (destInventory == null) return null;

            // Perform transfer
            sourceInventory.Quantity -= transfer.Quantity;
            sourceInventory.LastUpdated = DateTime.UtcNow;
            destInventory.Quantity += transfer.Quantity;
            destInventory.LastUpdated = DateTime.UtcNow;

            transfer.Status = "Completed";
            transfer.CompletedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return transfer;
        }

        #endregion

        #region Disposal Management

        public async Task<List<InventoryDisposal>> GetPendingDisposalsAsync()
        {
            return await GetDisposalsByStatusAsync("Pending");
        }

        public async Task<List<InventoryDisposal>> GetDisposalsByStatusAsync(string status)
        {
            return await _context.InventoryDisposals
                .Include(d => d.Inventory)
                    .ThenInclude(i => i.Product)
                .Include(d => d.Inventory)
                    .ThenInclude(i => i.FranchiseStore)
                .Include(d => d.Inventory)
                    .ThenInclude(i => i.CentralKitchen)
                .Include(d => d.DisposedBy)
                .Include(d => d.ApprovedBy)
                .Where(d => d.Status == status)
                .OrderByDescending(d => d.DisposalDate)
                .ToListAsync();
        }

        public async Task<List<InventoryDisposal>> GetDisposalHistoryAsync(int? storeId = null, int? centralKitchenId = null)
        {
            var query = _context.InventoryDisposals
                .Include(d => d.Inventory)
                    .ThenInclude(i => i.Product)
                .Include(d => d.Inventory)
                    .ThenInclude(i => i.FranchiseStore)
                .Include(d => d.Inventory)
                    .ThenInclude(i => i.CentralKitchen)
                .Include(d => d.DisposedBy)
                .Include(d => d.ApprovedBy)
                .AsQueryable();

            if (storeId.HasValue)
            {
                query = query.Where(d => d.Inventory.FranchiseStoreId == storeId);
            }
            if (centralKitchenId.HasValue)
            {
                query = query.Where(d => d.Inventory.CentralKitchenId == centralKitchenId);
            }

            return await query.OrderByDescending(d => d.DisposalDate).ToListAsync();
        }

        public async Task<InventoryDisposal?> GetDisposalByIdAsync(int disposalId)
        {
            return await _context.InventoryDisposals
                .Include(d => d.Inventory)
                    .ThenInclude(i => i.Product)
                .Include(d => d.Inventory)
                    .ThenInclude(i => i.FranchiseStore)
                .Include(d => d.Inventory)
                    .ThenInclude(i => i.CentralKitchen)
                .Include(d => d.DisposedBy)
                .Include(d => d.ApprovedBy)
                .FirstOrDefaultAsync(d => d.Id == disposalId);
        }

        public async Task<InventoryDisposal> RequestDisposalAsync(InventoryDisposal disposal)
        {
            disposal.Status = "Pending";
            disposal.DisposalDate = DateTime.UtcNow;
            _context.InventoryDisposals.Add(disposal);
            await _context.SaveChangesAsync();
            return disposal;
        }

        public async Task<InventoryDisposal?> ApproveDisposalAsync(int disposalId, Guid approvedByUserId)
        {
            var disposal = await _context.InventoryDisposals
                .Include(d => d.Inventory)
                .FirstOrDefaultAsync(d => d.Id == disposalId);

            if (disposal == null || disposal.Status != "Pending") return null;

            // Reduce inventory
            var inventory = disposal.Inventory;
            inventory.Quantity -= disposal.Quantity;
            if (inventory.Quantity < 0) inventory.Quantity = 0;
            inventory.LastUpdated = DateTime.UtcNow;

            disposal.Status = "Approved";
            disposal.ApprovedByUserId = approvedByUserId;
            disposal.ApprovedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return disposal;
        }

        public async Task<InventoryDisposal?> RejectDisposalAsync(int disposalId, Guid rejectedByUserId, string? reason = null)
        {
            var disposal = await _context.InventoryDisposals.FindAsync(disposalId);
            if (disposal == null || disposal.Status != "Pending") return null;

            disposal.Status = "Rejected";
            disposal.ApprovedByUserId = rejectedByUserId;
            disposal.ApprovedDate = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(reason))
            {
                disposal.Notes = (disposal.Notes ?? "") + $" [Rejected: {reason}]";
            }

            await _context.SaveChangesAsync();
            return disposal;
        }

        #endregion

        #region Dashboard Summary

        public async Task<InventoryDashboardSummary> GetDashboardSummaryAsync()
        {
            var now = DateTime.UtcNow;
            var expiryThreshold = now.AddDays(7);

            var centralKitchenItems = await _context.Inventories
                .Where(i => i.CentralKitchenId != null)
                .CountAsync();

            var storeItems = await _context.Inventories
                .Where(i => i.FranchiseStoreId != null)
                .CountAsync();

            var lowStockCount = await _context.Inventories
                .Where(i => i.Quantity <= i.MinThreshold)
                .CountAsync();

            var overstockCount = await _context.Inventories
                .Where(i => i.Quantity >= i.MaxThreshold)
                .CountAsync();

            var expiringCount = await _context.Inventories
                .Where(i => i.ExpiryDate != null && i.ExpiryDate > now && i.ExpiryDate <= expiryThreshold)
                .CountAsync();

            var expiredCount = await _context.Inventories
                .Where(i => i.ExpiryDate != null && i.ExpiryDate <= now)
                .CountAsync();

            var pendingTransfers = await _context.InventoryTransfers
                .Where(t => t.Status == "Pending")
                .CountAsync();

            var pendingDisposals = await _context.InventoryDisposals
                .Where(d => d.Status == "Pending")
                .CountAsync();

            var totalValue = await _context.Inventories
                .Include(i => i.Product)
                .SumAsync(i => i.Quantity * i.Product.Price);

            return new InventoryDashboardSummary
            {
                TotalCentralKitchenItems = centralKitchenItems,
                TotalStoreItems = storeItems,
                LowStockCount = lowStockCount,
                OverstockCount = overstockCount,
                ExpiringCount = expiringCount,
                ExpiredCount = expiredCount,
                PendingTransfers = pendingTransfers,
                PendingDisposals = pendingDisposals,
                TotalInventoryValue = totalValue
            };
        }

        #endregion
    }
}
