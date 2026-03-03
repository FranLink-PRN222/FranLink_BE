using System.Threading.Tasks;
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

        // Supply Coordinator flow
        Task<List<InternalOrder>> GetOrdersForSupplyAsync();
        Task ApproveOrderAsync(int orderId, int centralKitchenId);
        Task CancelOrderAsync(int orderId);
        Task StartProductionAsync(int orderId);
        Task MarkProductionReadyAsync(int orderId);
        Task StartOrAdvanceDeliveryAsync(int orderId);
        Task MarkDeliveredAsync(int orderId);
    }
}
