using BusinessLogicLayer_FranLink.DTOs;
using DataAccessLayer_FranLink.Models;

namespace BusinessLogicLayer_FranLink.Services
{
    public interface IProductionService
    {
        // CRUD Operations
        Task<ProductionRecord> StartProductionAsync(StartProductionDto dto, Guid userId);
        Task<ProductionRecord?> CompleteProductionAsync(int productionId, CompleteProductionDto dto);
        Task<ProductionRecord?> CancelProductionAsync(int productionId, string reason);
        Task<ProductionRecord?> GetByIdAsync(int productionId);

        // Queries
        Task<List<ProductionRecord>> GetProductionsByDateRangeAsync(DateTime from, DateTime to, int? centralKitchenId = null);
        Task<List<ProductionRecord>> GetProductionsByStatusAsync(string status);
        Task<List<ProductionRecord>> GetInProgressProductionsAsync(int? centralKitchenId = null);

        // Reports
        Task<ProductionSummary> GetProductionSummaryAsync(DateTime from, DateTime to, int? centralKitchenId = null);
        Task<List<ProductionByProduct>> GetTopProductsAsync(DateTime from, DateTime to, int top = 10);

        // Data
        Task<List<CentralKitchen>> GetAllCentralKitchensAsync();
        Task<List<Recipe>> GetActiveRecipesAsync();
    }
}
