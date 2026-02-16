using BusinessLogicLayer_FranLink.DTOs;

namespace BusinessLogicLayer_FranLink.Services
{
    public interface ILossService
    {
        Task<LossSummary> GetLossSummaryAsync(DateTime from, DateTime to, int? centralKitchenId = null, int? storeId = null);
        Task<List<LossByReason>> GetLossByReasonAsync(DateTime from, DateTime to, int? centralKitchenId = null, int? storeId = null);
        Task<List<LossByProduct>> GetLossByProductAsync(DateTime from, DateTime to, int? centralKitchenId = null, int? storeId = null);
        Task<List<LossByLocation>> GetLossByLocationAsync(DateTime from, DateTime to);
    }
}
