using BusinessLogicLayer_FranLink.DTOs;
using BusinessLogicLayer_FranLink.Services;
using DataAccessLayer_FranLink.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace PresentationLayer_FranLink.Pages.Manager.Operations
{
    public class IndexModel : PageModel
    {
        private readonly IProductionService _productionService;
        private readonly IDistributionService _distributionService;
        private readonly ILossService _lossService;
        private readonly IInventoryService _inventoryService;
        private readonly FranLinkContext _context;

        public IndexModel(
            IProductionService productionService,
            IDistributionService distributionService,
            ILossService lossService,
            IInventoryService inventoryService,
            FranLinkContext context)
        {
            _productionService = productionService;
            _distributionService = distributionService;
            _lossService = lossService;
            _inventoryService = inventoryService;
            _context = context;
        }

        // Production KPIs
        public ProductionSummary ProductionToday { get; set; } = new();
        public ProductionSummary ProductionThisWeek { get; set; } = new();

        // Distribution KPIs
        public DistributionSummary DistributionToday { get; set; } = new();
        public DistributionSummary DistributionThisWeek { get; set; } = new();

        // Loss KPIs
        public LossSummary LossThisMonth { get; set; } = new();

        // Inventory Alerts
        public InventoryDashboardSummary InventorySummary { get; set; } = new();
        public List<InProgressProductionItem> InProgressProductions { get; set; } = new();

        // Pending Items
        public int PendingOrders { get; set; }
        public int PendingDisposals { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Manager" && role != "Admin")
            {
                return RedirectToPage("/Login");
            }

            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);
            var weekStart = today.AddDays(-(int)today.DayOfWeek);
            var monthStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            // Production data
            ProductionToday = await _productionService.GetProductionSummaryAsync(today, tomorrow);
            ProductionThisWeek = await _productionService.GetProductionSummaryAsync(weekStart, tomorrow);

            // In progress productions
            var inProgress = await _productionService.GetInProgressProductionsAsync();
            InProgressProductions = inProgress.Select(p => new InProgressProductionItem
            {
                ProductionId = p.ProductionId,
                RecipeName = p.Recipe.Name,
                ProductName = p.Recipe.Product.Name,
                PlannedQuantity = p.PlannedQuantity,
                StartTime = p.StartTime,
                KitchenName = p.CentralKitchen.Name
            }).ToList();

            // Distribution data
            DistributionToday = await _distributionService.GetDistributionSummaryAsync(today, tomorrow);
            DistributionThisWeek = await _distributionService.GetDistributionSummaryAsync(weekStart, tomorrow);

            // Loss data
            LossThisMonth = await _lossService.GetLossSummaryAsync(monthStart, tomorrow);

            // Inventory summary
            InventorySummary = await _inventoryService.GetDashboardSummaryAsync();

            // Pending counts
            PendingOrders = await _context.InternalOrders
                .CountAsync(o => o.Status == "Pending" || o.Status == "Approved");
            PendingDisposals = await _context.InventoryDisposals
                .CountAsync(d => d.Status == "Pending");

            return Page();
        }
    }
}
