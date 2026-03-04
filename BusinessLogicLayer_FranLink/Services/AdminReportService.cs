using DataAccessLayer_FranLink.Models;
using BusinessLogicLayer_FranLink.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogicLayer_FranLink.Services
{
    public class AdminReportService : IAdminReportService
    {
        private readonly FranLinkContext _context;

        public AdminReportService(FranLinkContext context)
        {
            _context = context;
        }

        public async Task<AdminDashboardDto> GetDashboardAsync()
        {
            var dashboard = new AdminDashboardDto
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalActiveUsers = await _context.Users.CountAsync(u => u.IsActive),
                TotalStores = await _context.FranchiseStores.CountAsync(s => !s.IsCentralKitchen),
                TotalCentralKitchens = await _context.FranchiseStores.CountAsync(s => s.IsCentralKitchen),
                TotalProducts = await _context.Products.CountAsync(),
                TotalOrders = await _context.InternalOrders.CountAsync()
            };

            // Orders by status
            dashboard.OrdersByStatus = await _context.InternalOrders
                .GroupBy(o => o.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count);

            // Stores overview
            dashboard.StoresOverview = await _context.FranchiseStores
                .Select(s => new StoreOverviewItem
                {
                    StoreId = s.Id,
                    StoreName = s.Name,
                    IsCentralKitchen = s.IsCentralKitchen,
                    TotalOrders = s.InternalOrders.Count,
                    TotalInventoryItems = s.Inventories.Sum(i => i.Quantity)
                })
                .OrderByDescending(s => s.TotalOrders)
                .ToListAsync();

            return dashboard;
        }
    }
}
