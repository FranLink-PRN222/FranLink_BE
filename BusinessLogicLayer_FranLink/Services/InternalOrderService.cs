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

            // 4. Check Inventory
            foreach (var itemDto in dto.Items)
            {
                var totalQuantity = await _context.Inventories
                    .Where(i => i.ProductId == itemDto.ProductId)
                    .SumAsync(i => i.Quantity);

                if (totalQuantity < itemDto.Quantity)
                {
                    var product = await _context.Products.FindAsync(itemDto.ProductId);
                    string productName = product?.Name ?? $"ID {itemDto.ProductId}";
                    throw new Exception($"Insufficient inventory for product: {productName}. Available: {totalQuantity}, Requested: {itemDto.Quantity}");
                }
            }

            // 5. Create Order
            var order = new InternalOrder
            {
                FranchiseStoreId = dto.FranchiseStoreId,
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
    }
}
