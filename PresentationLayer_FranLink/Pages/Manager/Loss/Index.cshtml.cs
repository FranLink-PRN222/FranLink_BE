using BusinessLogicLayer_FranLink.DTOs;
using BusinessLogicLayer_FranLink.Services;
using DataAccessLayer_FranLink.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace PresentationLayer_FranLink.Pages.Manager.Loss
{
    public class IndexModel : PageModel
    {
        private readonly ILossService _lossService;
        private readonly FranLinkContext _context;

        public IndexModel(ILossService lossService, FranLinkContext context)
        {
            _lossService = lossService;
            _context = context;
        }

        public LossSummary Summary { get; set; } = new();
        public List<LossByReason> ByReason { get; set; } = new();
        public List<LossByProduct> TopProducts { get; set; } = new();
        public List<LossByLocation> ByLocation { get; set; } = new();
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
            if (role != "Manager" && role != "Admin")
            {
                return RedirectToPage("/Login");
            }

            FromDate ??= DateTime.UtcNow.AddDays(-30).Date;
            ToDate ??= DateTime.UtcNow.Date.AddDays(1);

            // Convert to UTC for PostgreSQL
            var fromDateUtc = DateTime.SpecifyKind(FromDate.Value, DateTimeKind.Utc);
            var toDateUtc = DateTime.SpecifyKind(ToDate.Value, DateTimeKind.Utc);

            Summary = await _lossService.GetLossSummaryAsync(
                fromDateUtc, toDateUtc, CentralKitchenId, null);

            ByReason = await _lossService.GetLossByReasonAsync(
                fromDateUtc, toDateUtc, CentralKitchenId, null);

            TopProducts = (await _lossService.GetLossByProductAsync(
                fromDateUtc, toDateUtc, CentralKitchenId, null)).Take(5).ToList();

            ByLocation = await _lossService.GetLossByLocationAsync(fromDateUtc, toDateUtc);

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
