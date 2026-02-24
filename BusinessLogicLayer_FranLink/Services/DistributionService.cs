using BusinessLogicLayer_FranLink.DTOs;
using DataAccessLayer_FranLink.Models;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogicLayer_FranLink.Services
{
    public class DistributionService : IDistributionService
    {
        private readonly FranLinkContext _context;

        public DistributionService(FranLinkContext context)
        {
            _context = context;
        }

        public async Task<DistributionSummary> GetDistributionSummaryAsync(
            DateTime from, DateTime to, int? centralKitchenId = null)
        {
            var query = _context.InternalOrders
                .Include(o => o.Items)
                .Where(o => o.OrderDate >= from && o.OrderDate <= to);

            if (centralKitchenId.HasValue)
                query = query.Where(o => o.CentralKitchenId == centralKitchenId.Value);

            var orders = await query.ToListAsync();

            var totalOrders = orders.Count;
            var completedOrders = orders.Count(o => o.Status == "Completed" || o.Status == "Delivered");
            var pendingOrders = orders.Count(o => o.Status == "Pending" || o.Status == "Approved");
            var deliveringOrders = orders.Count(o => o.Status == "Delivering");
            var cancelledOrders = orders.Count(o => o.Status == "Cancelled" || o.Status == "Rejected");

            var totalValue = orders
                .SelectMany(o => o.Items)
                .Sum(i => i.Quantity * i.UnitPrice);

            var summary = new DistributionSummary
            {
                TotalOrders = totalOrders,
                CompletedOrders = completedOrders,
                PendingOrders = pendingOrders,
                DeliveringOrders = deliveringOrders,
                CancelledOrders = cancelledOrders,
                TotalValue = totalValue,
                AverageOrderValue = totalOrders > 0 ? Math.Round(totalValue / totalOrders, 2) : 0,
                OrderFulfillmentRate = totalOrders > 0
                    ? Math.Round((decimal)completedOrders / totalOrders * 100, 2)
                    : 0,
                ByDay = orders
                    .GroupBy(o => o.OrderDate.Date)
                    .Select(g => new DistributionByDay
                    {
                        Date = g.Key,
                        OrderCount = g.Count(),
                        TotalValue = g.SelectMany(o => o.Items).Sum(i => i.Quantity * i.UnitPrice)
                    })
                    .OrderBy(d => d.Date)
                    .ToList()
            };

            return summary;
        }

        public async Task<List<DistributionByStore>> GetDistributionByStoreAsync(
            DateTime from, DateTime to, int? centralKitchenId = null)
        {
            var query = _context.InternalOrders
                .Include(o => o.FranchiseStore)
                .Include(o => o.Items)
                .Where(o => o.OrderDate >= from && o.OrderDate <= to);

            if (centralKitchenId.HasValue)
                query = query.Where(o => o.CentralKitchenId == centralKitchenId.Value);

            var orders = await query.ToListAsync();

            var result = orders
                .GroupBy(o => new { o.FranchiseStoreId, o.FranchiseStore.Name, o.FranchiseStore.Address })
                .Select(g =>
                {
                    var totalOrders = g.Count();
                    var completedOrders = g.Count(o => o.Status == "Completed" || o.Status == "Delivered");
                    return new DistributionByStore
                    {
                        StoreId = g.Key.FranchiseStoreId,
                        StoreName = g.Key.Name ?? "Unknown",
                        StoreAddress = g.Key.Address,
                        OrderCount = totalOrders,
                        TotalValue = g.SelectMany(o => o.Items).Sum(i => i.Quantity * i.UnitPrice),
                        ItemCount = g.SelectMany(o => o.Items).Sum(i => i.Quantity),
                        FulfillmentRate = totalOrders > 0
                            ? Math.Round((decimal)completedOrders / totalOrders * 100, 2)
                            : 0
                    };
                })
                .OrderByDescending(s => s.TotalValue)
                .ToList();

            return result;
        }

        public async Task<List<DistributionByProduct>> GetDistributionByProductAsync(
            DateTime from, DateTime to, int? centralKitchenId = null)
        {
            var query = _context.InternalOrders
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .Where(o => o.OrderDate >= from && o.OrderDate <= to);

            if (centralKitchenId.HasValue)
                query = query.Where(o => o.CentralKitchenId == centralKitchenId.Value);

            var orders = await query.ToListAsync();

            var result = orders
                .SelectMany(o => o.Items.Select(i => new { Order = o, Item = i }))
                .GroupBy(x => new { x.Item.ProductId, x.Item.Product.Name, x.Item.Product.Category })
                .Select(g =>
                {
                    var completedItems = g.Where(x => x.Order.Status == "Completed" || x.Order.Status == "Delivered");
                    return new DistributionByProduct
                    {
                        ProductId = g.Key.ProductId,
                        ProductName = g.Key.Name,
                        Category = g.Key.Category,
                        TotalQuantityOrdered = g.Sum(x => x.Item.Quantity),
                        TotalQuantityDelivered = completedItems.Sum(x => x.Item.Quantity),
                        TotalValue = g.Sum(x => x.Item.Quantity * x.Item.UnitPrice),
                        OrderCount = g.Select(x => x.Order.Id).Distinct().Count()
                    };
                })
                .OrderByDescending(p => p.TotalQuantityOrdered)
                .ToList();

            return result;
        }

        public async Task<DeliveryPerformance> GetDeliveryPerformanceAsync(
            DateTime from, DateTime to, int? centralKitchenId = null)
        {
            var query = _context.Deliveries
                .Include(d => d.InternalOrder)
                .Where(d => d.InternalOrder != null
                         && d.InternalOrder.OrderDate >= from
                         && d.InternalOrder.OrderDate <= to);

            if (centralKitchenId.HasValue)
                query = query.Where(d => d.InternalOrder!.CentralKitchenId == centralKitchenId.Value);

            var deliveries = await query.ToListAsync();

            var completedDeliveries = deliveries.Where(d => d.DeliveredAt.HasValue).ToList();
            var avgDeliveryTime = completedDeliveries.Any()
                ? completedDeliveries.Average(d =>
                    (d.DeliveredAt!.Value - d.InternalOrder.OrderDate).TotalHours)
                : 0;

            return new DeliveryPerformance
            {
                TotalDeliveries = deliveries.Count,
                CompletedDeliveries = completedDeliveries.Count,
                PendingDeliveries = deliveries.Count - completedDeliveries.Count,
                AverageDeliveryTimeHours = Math.Round((decimal)avgDeliveryTime, 2)
            };
        }
    }
}
