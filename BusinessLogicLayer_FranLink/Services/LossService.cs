using BusinessLogicLayer_FranLink.DTOs;
using DataAccessLayer_FranLink.Models;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogicLayer_FranLink.Services
{
    public class LossService : ILossService
    {
        private readonly FranLinkContext _context;

        public LossService(FranLinkContext context)
        {
            _context = context;
        }

        public async Task<LossSummary> GetLossSummaryAsync(
            DateTime from, DateTime to, int? centralKitchenId = null, int? storeId = null)
        {
            var query = _context.InventoryDisposals
                .Include(d => d.Inventory)
                    .ThenInclude(i => i.Product)
                .Where(d => d.DisposalDate >= from && d.DisposalDate <= to && d.Status == "Approved");

            if (centralKitchenId.HasValue)
                query = query.Where(d => d.Inventory.CentralKitchenId == centralKitchenId.Value);

            if (storeId.HasValue)
                query = query.Where(d => d.Inventory.FranchiseStoreId == storeId.Value);

            var disposals = await query.ToListAsync();

            var summary = new LossSummary
            {
                TotalDisposals = disposals.Count,
                TotalLossItems = disposals.Sum(d => d.Quantity),
                TotalLossValue = disposals.Sum(d => d.Quantity * d.Inventory.Product.Price),
                ExpiredLossValue = disposals
                    .Where(d => d.Reason.ToLower().Contains("expired") || d.Reason.ToLower().Contains("hết hạn"))
                    .Sum(d => d.Quantity * d.Inventory.Product.Price),
                DamagedLossValue = disposals
                    .Where(d => d.Reason.ToLower().Contains("damaged") || d.Reason.ToLower().Contains("hư hỏng"))
                    .Sum(d => d.Quantity * d.Inventory.Product.Price),
                QualityIssueLossValue = disposals
                    .Where(d => d.Reason.ToLower().Contains("quality") || d.Reason.ToLower().Contains("chất lượng"))
                    .Sum(d => d.Quantity * d.Inventory.Product.Price),
                OtherLossValue = disposals
                    .Where(d => !d.Reason.ToLower().Contains("expired")
                             && !d.Reason.ToLower().Contains("hết hạn")
                             && !d.Reason.ToLower().Contains("damaged")
                             && !d.Reason.ToLower().Contains("hư hỏng")
                             && !d.Reason.ToLower().Contains("quality")
                             && !d.Reason.ToLower().Contains("chất lượng"))
                    .Sum(d => d.Quantity * d.Inventory.Product.Price),
                ByDay = disposals
                    .GroupBy(d => d.DisposalDate.Date)
                    .Select(g => new LossByDay
                    {
                        Date = g.Key,
                        DisposalCount = g.Count(),
                        Quantity = g.Sum(d => d.Quantity),
                        Value = g.Sum(d => d.Quantity * d.Inventory.Product.Price)
                    })
                    .OrderBy(d => d.Date)
                    .ToList()
            };

            return summary;
        }

        public async Task<List<LossByReason>> GetLossByReasonAsync(
            DateTime from, DateTime to, int? centralKitchenId = null, int? storeId = null)
        {
            var query = _context.InventoryDisposals
                .Include(d => d.Inventory)
                    .ThenInclude(i => i.Product)
                .Where(d => d.DisposalDate >= from && d.DisposalDate <= to && d.Status == "Approved");

            if (centralKitchenId.HasValue)
                query = query.Where(d => d.Inventory.CentralKitchenId == centralKitchenId.Value);

            if (storeId.HasValue)
                query = query.Where(d => d.Inventory.FranchiseStoreId == storeId.Value);

            var disposals = await query.ToListAsync();
            var totalValue = disposals.Sum(d => d.Quantity * d.Inventory.Product.Price);

            var result = disposals
                .GroupBy(d => NormalizeReason(d.Reason))
                .Select(g => new LossByReason
                {
                    Reason = g.Key,
                    DisposalCount = g.Count(),
                    ItemCount = g.Sum(d => d.Quantity),
                    TotalValue = g.Sum(d => d.Quantity * d.Inventory.Product.Price),
                    Percentage = totalValue > 0
                        ? Math.Round(g.Sum(d => d.Quantity * d.Inventory.Product.Price) / totalValue * 100, 2)
                        : 0
                })
                .OrderByDescending(r => r.TotalValue)
                .ToList();

            return result;
        }

        public async Task<List<LossByProduct>> GetLossByProductAsync(
            DateTime from, DateTime to, int? centralKitchenId = null, int? storeId = null)
        {
            var query = _context.InventoryDisposals
                .Include(d => d.Inventory)
                    .ThenInclude(i => i.Product)
                .Where(d => d.DisposalDate >= from && d.DisposalDate <= to && d.Status == "Approved");

            if (centralKitchenId.HasValue)
                query = query.Where(d => d.Inventory.CentralKitchenId == centralKitchenId.Value);

            if (storeId.HasValue)
                query = query.Where(d => d.Inventory.FranchiseStoreId == storeId.Value);

            var disposals = await query.ToListAsync();

            var result = disposals
                .GroupBy(d => new { d.Inventory.ProductId, d.Inventory.Product.Name, d.Inventory.Product.Category })
                .Select(g =>
                {
                    var topReason = g.GroupBy(d => NormalizeReason(d.Reason))
                                     .OrderByDescending(rg => rg.Sum(d => d.Quantity))
                                     .First().Key;
                    return new LossByProduct
                    {
                        ProductId = g.Key.ProductId,
                        ProductName = g.Key.Name,
                        Category = g.Key.Category,
                        LostQuantity = g.Sum(d => d.Quantity),
                        LostValue = g.Sum(d => d.Quantity * d.Inventory.Product.Price),
                        TopReason = topReason,
                        DisposalCount = g.Count()
                    };
                })
                .OrderByDescending(p => p.LostValue)
                .ToList();

            return result;
        }

        public async Task<List<LossByLocation>> GetLossByLocationAsync(DateTime from, DateTime to)
        {
            var disposals = await _context.InventoryDisposals
                .Include(d => d.Inventory)
                    .ThenInclude(i => i.Product)
                .Include(d => d.Inventory)
                    .ThenInclude(i => i.CentralKitchen)
                .Include(d => d.Inventory)
                    .ThenInclude(i => i.FranchiseStore)
                .Where(d => d.DisposalDate >= from && d.DisposalDate <= to && d.Status == "Approved")
                .ToListAsync();

            var result = new List<LossByLocation>();

            // Group by Central Kitchen
            var byCentralKitchen = disposals
                .Where(d => d.Inventory.CentralKitchenId.HasValue)
                .GroupBy(d => new { d.Inventory.CentralKitchenId, d.Inventory.CentralKitchen!.Name })
                .Select(g => new LossByLocation
                {
                    CentralKitchenId = g.Key.CentralKitchenId,
                    LocationName = g.Key.Name ?? "Unknown",
                    LocationType = "Central Kitchen",
                    DisposalCount = g.Count(),
                    TotalQuantity = g.Sum(d => d.Quantity),
                    TotalValue = g.Sum(d => d.Quantity * d.Inventory.Product.Price)
                });

            result.AddRange(byCentralKitchen);

            // Group by Franchise Store
            var byStore = disposals
                .Where(d => d.Inventory.FranchiseStoreId.HasValue)
                .GroupBy(d => new { d.Inventory.FranchiseStoreId, d.Inventory.FranchiseStore!.Name })
                .Select(g => new LossByLocation
                {
                    FranchiseStoreId = g.Key.FranchiseStoreId,
                    LocationName = g.Key.Name ?? "Unknown",
                    LocationType = "Franchise Store",
                    DisposalCount = g.Count(),
                    TotalQuantity = g.Sum(d => d.Quantity),
                    TotalValue = g.Sum(d => d.Quantity * d.Inventory.Product.Price)
                });

            result.AddRange(byStore);

            return result.OrderByDescending(l => l.TotalValue).ToList();
        }

        private static string NormalizeReason(string reason)
        {
            var lower = reason.ToLower();
            if (lower.Contains("expired") || lower.Contains("hết hạn"))
                return "Expired";
            if (lower.Contains("damaged") || lower.Contains("hư hỏng"))
                return "Damaged";
            if (lower.Contains("quality") || lower.Contains("chất lượng"))
                return "Quality Issue";
            return "Other";
        }
    }
}
