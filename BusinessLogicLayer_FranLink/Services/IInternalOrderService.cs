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

        // Central Kitchen staff flow (giữ nguyên cho trang CentralKitchenStaff đang dùng)
        Task ApproveOrderAsync(int orderId);
        Task RejectOrderAsync(int orderId, string? reason = null);
        Task StartPreparingOrderAsync(int orderId);
        Task MarkAsDeliveringAsync(int orderId);
        Task MarkDeliveryCompletedAsync(int orderId);

        // Supply Coordinator flow
        Task<List<InternalOrder>> GetOrdersForSupplyAsync();
        Task ApproveOrderForSupplyAsync(int orderId);
        Task CancelOrderAsync(int orderId);
        Task StartProductionAsync(int orderId);
        Task MarkProductionReadyAsync(int orderId);
        Task StartOrAdvanceDeliveryAsync(int orderId, DateTime? scheduledDate = null);
        Task MarkDeliveredAsync(int orderId);
    }
}
