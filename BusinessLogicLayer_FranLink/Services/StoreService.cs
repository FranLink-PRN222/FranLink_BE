using DataAccessLayer_FranLink.Models;
using BusinessLogicLayer_FranLink.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogicLayer_FranLink.Services
{
    public class StoreService : IStoreService
    {
        private readonly FranLinkContext _context;

        public StoreService(FranLinkContext context)
        {
            _context = context;
        }

        public async Task<List<FranchiseStore>> GetAllStoresAsync()
        {
            return await _context.FranchiseStores
                .OrderByDescending(s => s.CreatedDate)
                .ToListAsync();
        }

        public async Task<FranchiseStore> GetStoreByIdAsync(int id)
        {
            return await _context.FranchiseStores
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<FranchiseStore> CreateStoreAsync(CreateStoreDto dto)
        {
            var store = new FranchiseStore
            {
                Name = dto.Name,
                Address = dto.Address,
                Phone = dto.Phone,
                IsCentralKitchen = dto.IsCentralKitchen,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            _context.FranchiseStores.Add(store);
            await _context.SaveChangesAsync();
            return store;
        }

        public async Task UpdateStoreAsync(UpdateStoreDto dto)
        {
            var store = await _context.FranchiseStores.FindAsync(dto.Id);
            if (store == null)
                throw new InvalidOperationException("Store not found.");

            store.Name = dto.Name;
            store.Address = dto.Address;
            store.Phone = dto.Phone;
            store.IsCentralKitchen = dto.IsCentralKitchen;
            store.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();
        }

        public async Task ToggleStoreActiveAsync(int storeId)
        {
            var store = await _context.FranchiseStores.FindAsync(storeId);
            if (store == null)
                throw new InvalidOperationException("Store not found.");

            store.IsActive = !store.IsActive;
            await _context.SaveChangesAsync();
        }
    }
}
