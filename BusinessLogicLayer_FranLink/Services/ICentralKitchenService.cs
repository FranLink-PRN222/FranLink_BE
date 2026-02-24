using BusinessLogicLayer_FranLink.DTOs;
using DataAccessLayer_FranLink.Models;

namespace BusinessLogicLayer_FranLink.Services
{
    public interface ICentralKitchenService
    {
        Task<List<CentralKitchen>> GetAllCentralKitchensAsync();
        Task<CentralKitchen?> GetCentralKitchenByIdAsync(int id);
        Task<CentralKitchen> CreateCentralKitchenAsync(CentralKitchen centralKitchen);
        Task<CentralKitchen?> UpdateCentralKitchenAsync(CentralKitchen centralKitchen);
        Task<bool> DeleteCentralKitchenAsync(int id);
        Task<CentralKitchenSummary> GetCentralKitchenSummaryAsync(int id);
    }
}
