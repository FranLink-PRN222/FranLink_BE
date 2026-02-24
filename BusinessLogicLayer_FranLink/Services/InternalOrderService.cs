using System;
using System.Linq;
using System.Threading.Tasks;
using BusinessLogicLayer_FranLink.DTOs;
using DataAccessLayer_FranLink.Models;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogicLayer_FranLink.Services
{
    public class InternalOrderService : IInternalOrderService
    {
        private readonly FranLinkContext _context;

        public InternalOrderService(FranLinkContext context)
        {
            _context = context;
        }

        public async Task<InternalOrder> CreateOrderAsync(CreateInternalOrderDto dto)
        {
            // 1. Validate Store
            var store = await _context.FranchiseStores.FindAsync(dto.FranchiseStoreId);
            if (store == null)
            {
                throw new Exception("Franchise Store not found.");
            }

            // 2. Validate Central Kitchen
            var centralKitchen = await _context.CentralKitchens.FindAsync(dto.CentralKitchenId);
            if (centralKitchen == null)
            {
                throw new Exception("Central Kitchen not found.");
            }

            // 3. Check Central Kitchen Inventory
            foreach (var itemDto in dto.Items)
            {
                var totalQuantity = await _context.Inventories
                    .Where(i => i.CentralKitchenId == dto.CentralKitchenId && i.ProductId == itemDto.ProductId)
                    .SumAsync(i => i.Quantity);

                if (totalQuantity < itemDto.Quantity)
                {
                    var product = await _context.Products.FindAsync(itemDto.ProductId);
                    string productName = product?.Name ?? $"ID {itemDto.ProductId}";
                    throw new Exception($"Insufficient inventory for product: {productName}. Available: {totalQuantity}, Requested: {itemDto.Quantity}");
                }
            }

            // 4. Create Order
            var order = new InternalOrder
            {
                FranchiseStoreId = dto.FranchiseStoreId,
                CentralKitchenId = dto.CentralKitchenId,
                UserId = dto.UserId,
                OrderDate = DateTime.UtcNow,
                Status = "Pending",
                Items = new List<InternalOrderItem>()
            };

            // Create associated Delivery record
            // InternalOrderId will be automatically set by EF Core when linking via navigation property
            var delivery = new Delivery
            {
                DeliveryId = Guid.NewGuid(),
                DeliveryStatus = "Pending",
                DeliveredAt = null
            };
            order.Delivery = delivery;

            foreach (var itemDto in dto.Items)
            {
                var product = await _context.Products.FindAsync(itemDto.ProductId);
                // Product existence implies price availability
                 
                var orderItem = new InternalOrderItem
                {
                    ProductId = itemDto.ProductId,
                    Quantity = itemDto.Quantity,
                    UnitPrice = product!.Price // Validated existence above logic if strict, but safe here
                };
                order.Items.Add(orderItem);
            }

            _context.InternalOrders.Add(order);
            await _context.SaveChangesAsync();

            return order;
        }

        public async Task<List<InternalOrder>> GetOrdersByStoreIdAsync(int storeId)
        {
            return await _context.InternalOrders
                .Include(o => o.Delivery)
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .Where(o => o.FranchiseStoreId == storeId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<InternalOrder> GetOrderByIdAsync(int orderId)
        {
            return await _context.InternalOrders
                .Include(o => o.Delivery)
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task ConfirmOrderReceivedAsync(int orderId)
        {
            var order = await _context.InternalOrders
                .Include(o => o.Delivery)
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) throw new Exception("Order not found.");

            // Validation: Order must be in a state where it can be received.
            // Assuming "Completed" delivery means ready to receive, or strictly following workflow.
            if (order.Delivery == null || order.Delivery.DeliveryStatus != "Completed")
            {
                throw new Exception("Order cannot be confirmed. Delivery is not completed.");
            }

            if (order.Status == "Completed")
            {
                throw new Exception("Order is already completed.");
            }

            // Update Order Status
            order.Status = "Completed";

            // Update Inventory
            foreach (var item in order.Items)
            {
                var inventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.FranchiseStoreId == order.FranchiseStoreId && i.ProductId == item.ProductId);

                if (inventory == null)
                {
                    // Create new inventory record if it doesn't exist
                    inventory = new Inventory
                    {
                        FranchiseStoreId = order.FranchiseStoreId,
                        ProductId = item.ProductId,
                        Quantity = 0
                    };
                    _context.Inventories.Add(inventory);
                }

                inventory.Quantity += item.Quantity;
            }

            await _context.SaveChangesAsync();
        }

        public async Task AddFeedbackAsync(QualityFeedback feedback)
        {
             // Validate if needed
             _context.QualityFeedbacks.Add(feedback);
             await _context.SaveChangesAsync();
        }

        public async Task<List<InternalOrder>> GetOrdersByCentralKitchenIdAsync(int centralKitchenId, string? statusFilter = null)
        {
            var query = _context.InternalOrders
                .Include(o => o.Delivery)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .Include(o => o.FranchiseStore)
                .Include(o => o.User)
                .Where(o => o.CentralKitchenId == centralKitchenId);

            if (!string.IsNullOrEmpty(statusFilter))
            {
                if (statusFilter == "Pending")
                    query = query.Where(o => o.Status == "Pending");
                else if (statusFilter == "Approved" || statusFilter == "Producing")
                    query = query.Where(o => o.Status == "Approved" || o.Status == "Producing");
                else if (statusFilter == "Delivering")
                    query = query.Where(o => o.Status == "Delivering" || (o.Delivery != null && o.Delivery.DeliveryStatus == "Delivering"));
                else if (statusFilter == "Completed")
                    query = query.Where(o => o.Status == "Completed");
                else if (statusFilter == "Cancelled" || statusFilter == "Rejected")
                    query = query.Where(o => o.Status == "Rejected" || o.Status == "Cancelled");
            }

            return await query.OrderByDescending(o => o.OrderDate).ToListAsync();
        }

        public async Task ApproveOrderAsync(int orderId)
        {
            var order = await _context.InternalOrders
                .Include(o => o.Delivery)
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) throw new Exception("Order not found.");
            if (order.Status != "Pending") throw new Exception("Only pending orders can be approved.");

            order.Status = "Approved";
            if (order.Delivery != null)
                order.Delivery.DeliveryStatus = "Preparing";

            await _context.SaveChangesAsync();
        }

        public async Task RejectOrderAsync(int orderId, string? reason = null)
        {
            var order = await _context.InternalOrders
                .Include(o => o.Delivery)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) throw new Exception("Order not found.");
            if (order.Status != "Pending") throw new Exception("Only pending orders can be rejected.");

            order.Status = "Rejected";
            if (order.Delivery != null)
                order.Delivery.DeliveryStatus = "Cancelled";

            await _context.SaveChangesAsync();
        }

        public async Task StartPreparingOrderAsync(int orderId)
        {
            var order = await _context.InternalOrders
                .Include(o => o.Delivery)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) throw new Exception("Order not found.");
            if (order.Status != "Approved") throw new Exception("Order must be approved first.");

            order.Status = "Producing";
            if (order.Delivery != null)
                order.Delivery.DeliveryStatus = "Preparing";

            await _context.SaveChangesAsync();
        }

        public async Task MarkAsDeliveringAsync(int orderId)
        {
            var order = await _context.InternalOrders
                .Include(o => o.Delivery)
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) throw new Exception("Order not found.");
            if (order.CentralKitchenId == null) throw new Exception("Order must be from a Central Kitchen.");

            // Deduct inventory from Central Kitchen
            foreach (var item in order.Items)
            {
                var inventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.CentralKitchenId == order.CentralKitchenId && i.ProductId == item.ProductId);

                if (inventory == null || inventory.Quantity < item.Quantity)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    throw new Exception($"Insufficient inventory for {product?.Name ?? "product"}. Available: {inventory?.Quantity ?? 0}, Required: {item.Quantity}");
                }

                inventory.Quantity -= item.Quantity;
                inventory.LastUpdated = DateTime.UtcNow;
            }

            order.Status = "Delivering";
            if (order.Delivery != null)
            {
                order.Delivery.DeliveryStatus = "Delivering";
            }

            await _context.SaveChangesAsync();
        }

        public async Task MarkDeliveryCompletedAsync(int orderId)
        {
            var order = await _context.InternalOrders
                .Include(o => o.Delivery)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) throw new Exception("Order not found.");
            if (order.Delivery == null) throw new Exception("Delivery record not found.");
            if (order.Delivery.DeliveryStatus != "Delivering") throw new Exception("Order must be in Delivering status.");

            order.Delivery.DeliveryStatus = "Completed";
            order.Delivery.DeliveredAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
    }
}
