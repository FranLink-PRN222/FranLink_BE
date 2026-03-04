using BusinessLogicLayer_FranLink.DTOs;
using DataAccessLayer_FranLink.Models;

namespace BusinessLogicLayer_FranLink.Services
{
    public interface IInternalOrderService
    {
        Task<InternalOrder> CreateOrderAsync(CreateInternalOrderDto dto);
        Task<List<InternalOrder>> GetOrdersByStoreIdAsync(int storeId);
        Task<InternalOrder> GetOrderByIdAsync(int orderId);
        Task ConfirmOrderReceivedAsync(int orderId);
        Task AddFeedbackAsync(QualityFeedback feedback);
        Task<List<InternalOrder>> GetOrdersByCentralKitchenIdAsync(int centralKitchenId, string? statusFilter = null);


        /// <summary>
        /// Central Kitchen Staff: Approve pending order.
        /// </summary>
        Task ApproveOrderAsync(int orderId);

        /// <summary>
        /// Central Kitchen Staff: Reject pending order.
        /// </summary>
        Task RejectOrderAsync(int orderId, string? reason = null);

        /// <summary>
        /// Central Kitchen Staff: Start preparing/fulfilling order (Status -> Approved/Producing).
        /// </summary>
        Task StartPreparingOrderAsync(int orderId);

        /// <summary>
        /// Central Kitchen Staff: Mark order as out for delivery. Deducts from Central Kitchen inventory.
        /// </summary>
        Task MarkAsDeliveringAsync(int orderId);

        /// <summary>
        /// Central Kitchen Staff: Mark delivery as completed (arrived at store).
        /// </summary>
        Task MarkDeliveryCompletedAsync(int orderId);
    }
}
