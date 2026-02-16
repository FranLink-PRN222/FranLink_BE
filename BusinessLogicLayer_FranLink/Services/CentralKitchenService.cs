using BusinessLogicLayer_FranLink.DTOs;
using DataAccessLayer_FranLink.Models;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogicLayer_FranLink.Services
{
    public class CentralKitchenService : ICentralKitchenService
    {
        private readonly FranLinkContext _context;

        public CentralKitchenService(FranLinkContext context)
        {
            _context = context;
        }

        public async Task<List<CentralKitchen>> GetAllCentralKitchensAsync()
        {
            return await _context.CentralKitchens
                .Include(ck => ck.Inventories)
                    .ThenInclude(i => i.Product)
                .OrderBy(ck => ck.Name)
                .ToListAsync();
        }

        public async Task<CentralKitchen?> GetCentralKitchenByIdAsync(int id)
        {
            return await _context.CentralKitchens
                .Include(ck => ck.Inventories)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(ck => ck.Id == id);
        }

        public async Task<CentralKitchen> CreateCentralKitchenAsync(CentralKitchen centralKitchen)
        {
            centralKitchen.CreatedDate = DateTime.UtcNow;
            _context.CentralKitchens.Add(centralKitchen);
            await _context.SaveChangesAsync();
            return centralKitchen;
        }

        public async Task<CentralKitchen?> UpdateCentralKitchenAsync(CentralKitchen centralKitchen)
        {
            var existing = await _context.CentralKitchens.FindAsync(centralKitchen.Id);
            if (existing == null) return null;

            existing.Name = centralKitchen.Name;
            existing.Address = centralKitchen.Address;
            existing.Capacity = centralKitchen.Capacity;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteCentralKitchenAsync(int id)
        {
            var centralKitchen = await _context.CentralKitchens.FindAsync(id);
            if (centralKitchen == null) return false;

            // Check if there's inventory
            var hasInventory = await _context.Inventories
                .AnyAsync(i => i.CentralKitchenId == id);

            if (hasInventory)
            {
                return false; // Cannot delete central kitchen with inventory
            }

            _context.CentralKitchens.Remove(centralKitchen);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<CentralKitchenSummary> GetCentralKitchenSummaryAsync(int id)
        {
            var centralKitchen = await _context.CentralKitchens
                .Include(ck => ck.Inventories)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(ck => ck.Id == id);

            if (centralKitchen == null)
            {
                return new CentralKitchenSummary { CentralKitchenId = id };
            }

            var now = DateTime.UtcNow;
            var expiryThreshold = now.AddDays(7);

            return new CentralKitchenSummary
            {
                CentralKitchenId = id,
                Name = centralKitchen.Name,
                TotalProducts = centralKitchen.Inventories.Select(i => i.ProductId).Distinct().Count(),
                TotalQuantity = centralKitchen.Inventories.Sum(i => i.Quantity),
                LowStockItems = centralKitchen.Inventories.Count(i => i.Quantity <= i.MinThreshold),
                ExpiringItems = centralKitchen.Inventories.Count(i => i.ExpiryDate != null && i.ExpiryDate > now && i.ExpiryDate <= expiryThreshold),
                ExpiredItems = centralKitchen.Inventories.Count(i => i.ExpiryDate != null && i.ExpiryDate <= now),
                TotalValue = centralKitchen.Inventories.Sum(i => i.Quantity * i.Product.Price)
            };
        }
    }
}
