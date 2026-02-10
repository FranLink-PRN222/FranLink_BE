using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccessLayer_FranLink.Models;

namespace BusinessLogicLayer_FranLink.Services
{
    public interface IInventoryService
    {
        Task<List<Inventory>> GetInventoryByStoreIdAsync(int storeId);
    }
}
