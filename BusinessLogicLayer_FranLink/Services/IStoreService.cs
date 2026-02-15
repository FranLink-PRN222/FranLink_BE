using DataAccessLayer_FranLink.Models;
using BusinessLogicLayer_FranLink.DTOs;

namespace BusinessLogicLayer_FranLink.Services
{
    public interface IStoreService
    {
        Task<List<FranchiseStore>> GetAllStoresAsync();
        Task<FranchiseStore> GetStoreByIdAsync(int id);
        Task<FranchiseStore> CreateStoreAsync(CreateStoreDto dto);
        Task UpdateStoreAsync(UpdateStoreDto dto);
        Task ToggleStoreActiveAsync(int storeId);
    }
}
