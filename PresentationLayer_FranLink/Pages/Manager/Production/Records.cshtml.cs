using BusinessLogicLayer_FranLink.Services;
using DataAccessLayer_FranLink.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PresentationLayer_FranLink.Pages.Manager.Production
{
    public class RecordsModel : PageModel
    {
        private readonly IProductionService _productionService;

        public RecordsModel(IProductionService productionService)
        {
            _productionService = productionService;
        }

        public List<ProductionRecord> Productions { get; set; } = new();
        public List<SelectListItem> CentralKitchens { get; set; } = new();
        public List<SelectListItem> Statuses { get; set; } = new()
        {
            new("All", ""),
            new("In Progress", "InProgress"),
            new("Completed", "Completed"),
            new("Cancelled", "Cancelled")
        };

        [BindProperty(SupportsGet = true)]
        public DateTime? FromDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? ToDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? CentralKitchenId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Status { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Manager" && role != "Admin" && role != "CentralKitchenStaff")
            {
                return RedirectToPage("/Login");
            }

            // Default date range
            if (!FromDate.HasValue)
                FromDate = DateTime.UtcNow.AddDays(-7).Date;
            if (!ToDate.HasValue)
                ToDate = DateTime.UtcNow.Date.AddDays(1);

            // Convert to UTC to fix PostgreSQL timestamp with time zone error
            var fromDateUtc = DateTime.SpecifyKind(FromDate.Value, DateTimeKind.Utc);
            var toDateUtc = DateTime.SpecifyKind(ToDate.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);

            var productions = await _productionService.GetProductionsByDateRangeAsync(
                fromDateUtc, toDateUtc, CentralKitchenId);

            if (!string.IsNullOrEmpty(Status))
            {
                productions = productions.Where(p => p.Status == Status).ToList();
            }

            Productions = productions;

            var kitchens = await _productionService.GetAllCentralKitchensAsync();
            CentralKitchens = kitchens.Select(ck => new SelectListItem
            {
                Value = ck.Id.ToString(),
                Text = ck.Name
            }).ToList();

            return Page();
        }
    }
}
