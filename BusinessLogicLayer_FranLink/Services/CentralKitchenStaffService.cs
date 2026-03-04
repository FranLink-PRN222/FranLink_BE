using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessLogicLayer_FranLink.DTOs;
using DataAccessLayer_FranLink.Models;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogicLayer_FranLink.Services
{
    public class CentralKitchenStaffService : ICentralKitchenStaffService
    {
        private readonly FranLinkContext _context;

        public CentralKitchenStaffService(FranLinkContext context)
        {
            _context = context;
        }

        public async Task<List<AggregatedDemandItem>> GetAggregatedDemandAsync(int centralKitchenId)
        {
            var orders = await _context.InternalOrders
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .Where(o => o.CentralKitchenId == centralKitchenId
                         && (o.Status == "Pending" || o.Status == "Approved" || o.Status == "Producing"))
                .ToListAsync();

            var demandByProduct = orders
                .SelectMany(o => o.Items.Select(i => new { OrderId = o.Id, i.ProductId, i.Quantity, i.Product }))
                .GroupBy(x => new { x.ProductId, x.Product.Name, x.Product.Category })
                .Select(g => new
                {
                    g.Key.ProductId,
                    g.Key.Name,
                    g.Key.Category,
                    TotalQuantity = g.Sum(x => x.Quantity),
                    OrderCount = g.Select(x => x.OrderId).Distinct().Count()
                })
                .ToList();

            var inventorySums = await _context.Inventories
                .Where(i => i.CentralKitchenId == centralKitchenId)
                .GroupBy(i => i.ProductId)
                .Select(g => new { ProductId = g.Key, Available = g.Sum(i => i.Quantity) })
                .ToDictionaryAsync(x => x.ProductId, x => x.Available);

            return demandByProduct.Select(d => new AggregatedDemandItem
            {
                ProductId = d.ProductId,
                ProductName = d.Name ?? "Unknown",
                Category = d.Category,
                TotalQuantityDemanded = d.TotalQuantity,
                OrderCount = d.OrderCount,
                AvailableInInventory = inventorySums.TryGetValue(d.ProductId, out var avail) ? avail : 0
            }).OrderByDescending(x => x.ShortageQuantity).ToList();
        }

        public async Task<List<MaterialWithExpiryDto>> GetMaterialsWithExpiryAsync(int centralKitchenId, bool? expiringOnly = null)
        {
            var now = DateTime.UtcNow.Date;
            var expiryThreshold = now.AddDays(7);

            var query = _context.Inventories
                .Include(i => i.Product)
                .Where(i => i.CentralKitchenId == centralKitchenId);

            if (expiringOnly == true)
            {
                query = query.Where(i => i.ExpiryDate != null
                    && (i.ExpiryDate <= expiryThreshold || i.ExpiryDate <= now));
            }

            var items = await query.OrderBy(i => i.ExpiryDate).ToListAsync();

            return items.Select(i =>
            {
                int daysUntil = i.ExpiryDate.HasValue
                    ? (int)(i.ExpiryDate.Value.Date - now).TotalDays
                    : int.MaxValue;
                string status = !i.ExpiryDate.HasValue ? "Normal"
                    : i.ExpiryDate <= now ? "Expired"
                    : i.ExpiryDate <= expiryThreshold ? "ExpiringSoon"
                    : "Normal";

                return new MaterialWithExpiryDto
                {
                    InventoryId = i.InventoryId,
                    ProductId = i.ProductId,
                    ProductName = i.Product?.Name ?? "Unknown",
                    Category = i.Product?.Category,
                    Quantity = i.Quantity,
                    BatchNumber = i.BatchNumber,
                    ExpiryDate = i.ExpiryDate,
                    DaysUntilExpiry = daysUntil == int.MaxValue ? 9999 : daysUntil,
                    Status = status
                };
            }).ToList();
        }

        public async Task<List<CentralKitchen>> GetAllCentralKitchensAsync()
        {
            return await _context.CentralKitchens.OrderBy(ck => ck.Name).ToListAsync();
        }
    }
}
