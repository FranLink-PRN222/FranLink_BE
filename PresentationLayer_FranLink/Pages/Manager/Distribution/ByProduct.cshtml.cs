using BusinessLogicLayer_FranLink.DTOs;
using BusinessLogicLayer_FranLink.Services;
using DataAccessLayer_FranLink.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace PresentationLayer_FranLink.Pages.Manager.Distribution
{
    public class ByProductModel : PageModel
    {
        private readonly IDistributionService _distributionService;
        private readonly FranLinkContext _context;

        public ByProductModel(IDistributionService distributionService, FranLinkContext context)
        {
            _distributionService = distributionService;
            _context = context;
        }

        public List<DistributionByProduct> Products { get; set; } = new();
        public List<SelectListItem> CentralKitchens { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public DateTime? FromDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? ToDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? CentralKitchenId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Category { get; set; }

        public List<string> Categories { get; set; } = new();

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

            var products = await _distributionService.GetDistributionByProductAsync(
                fromDateUtc, toDateUtc, CentralKitchenId);

            Categories = products
                .Where(p => !string.IsNullOrEmpty(p.Category))
                .Select(p => p.Category!)
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            if (!string.IsNullOrEmpty(Category))
            {
                products = products.Where(p => p.Category == Category).ToList();
            }

            Products = products;

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
