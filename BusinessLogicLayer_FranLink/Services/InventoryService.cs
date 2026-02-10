using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccessLayer_FranLink.Models;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogicLayer_FranLink.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly FranLinkContext _context;

        public InventoryService(FranLinkContext context)
        {
            _context = context;
        }

        public async Task<List<Inventory>> GetInventoryByStoreIdAsync(int storeId)
        {
            return await _context.Inventories
                .Include(i => i.Product)
                .Where(i => i.FranchiseStoreId == storeId)
                .ToListAsync();
        }
    }
}
