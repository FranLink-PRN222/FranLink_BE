using BusinessLogicLayer_FranLink.DTOs;

namespace BusinessLogicLayer_FranLink.Services
{
    public interface IDistributionService
    {
        Task<DistributionSummary> GetDistributionSummaryAsync(DateTime from, DateTime to, int? centralKitchenId = null);
        Task<List<DistributionByStore>> GetDistributionByStoreAsync(DateTime from, DateTime to, int? centralKitchenId = null);
        Task<List<DistributionByProduct>> GetDistributionByProductAsync(DateTime from, DateTime to, int? centralKitchenId = null);
        Task<DeliveryPerformance> GetDeliveryPerformanceAsync(DateTime from, DateTime to, int? centralKitchenId = null);
    }
}
