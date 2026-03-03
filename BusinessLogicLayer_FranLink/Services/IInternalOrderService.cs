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
    }
}
