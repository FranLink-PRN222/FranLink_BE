using BusinessLogicLayer_FranLink.DTOs;
using BusinessLogicLayer_FranLink.Services;
using DataAccessLayer_FranLink.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace PresentationLayer_FranLink.Pages.Manager.Distribution
{
    public class IndexModel : PageModel
    {
        private readonly IDistributionService _distributionService;
        private readonly FranLinkContext _context;

        public IndexModel(IDistributionService distributionService, FranLinkContext context)
        {
            _distributionService = distributionService;
            _context = context;
        }

        public DistributionSummary Summary { get; set; } = new();
        public List<DistributionByStore> TopStores { get; set; } = new();
        public List<DistributionByProduct> TopProducts { get; set; } = new();
        public DeliveryPerformance DeliveryStats { get; set; } = new();
        public List<SelectListItem> CentralKitchens { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public DateTime? FromDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? ToDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? CentralKitchenId { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Manager")
            {
                return RedirectToPage("/Login");
            }

            FromDate ??= DateTime.UtcNow.AddDays(-30).Date;
            ToDate ??= DateTime.UtcNow.Date.AddDays(1);

            // Convert to UTC for PostgreSQL
            var fromDateUtc = DateTime.SpecifyKind(FromDate.Value, DateTimeKind.Utc);
            var toDateUtc = DateTime.SpecifyKind(ToDate.Value, DateTimeKind.Utc);

            Summary = await _distributionService.GetDistributionSummaryAsync(
                fromDateUtc, toDateUtc, CentralKitchenId);

            TopStores = (await _distributionService.GetDistributionByStoreAsync(
                fromDateUtc, toDateUtc, CentralKitchenId)).Take(5).ToList();

            TopProducts = (await _distributionService.GetDistributionByProductAsync(
                fromDateUtc, toDateUtc, CentralKitchenId)).Take(5).ToList();

            DeliveryStats = await _distributionService.GetDeliveryPerformanceAsync(
                fromDateUtc, toDateUtc, CentralKitchenId);

            var kitchens = await _context.CentralKitchens.OrderBy(ck => ck.Name).ToListAsync();
            CentralKitchens = kitchens.Select(ck => new SelectListItem
            {
                Value = ck.Id.ToString(),
                Text = ck.Name
            }).ToList();

            return Page();
        }
    }
}
