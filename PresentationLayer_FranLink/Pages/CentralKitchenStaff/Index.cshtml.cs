using BusinessLogicLayer_FranLink.DTOs;
using BusinessLogicLayer_FranLink.Services;
using DataAccessLayer_FranLink.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PresentationLayer_FranLink.Pages.CentralKitchenStaff
{
    public class IndexModel : PageModel
    {
        private readonly IInternalOrderService _orderService;
        private readonly ICentralKitchenStaffService _ckStaffService;
        private readonly IProductionService _productionService;

        public IndexModel(
            IInternalOrderService orderService,
            ICentralKitchenStaffService ckStaffService,
            IProductionService productionService)
        {
            _orderService = orderService;
            _ckStaffService = ckStaffService;
            _productionService = productionService;
        }

        public int PendingOrders { get; set; }
        public int PreparingOrders { get; set; }
        public int DeliveringOrders { get; set; }
        public List<AggregatedDemandItem> TopDemand { get; set; } = new();
        public List<InProgressProductionItem> InProgressProductions { get; set; } = new();
        public List<SelectListItem> CentralKitchens { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int? CentralKitchenId { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "CentralKitchenStaff" && role != "Manager" && role != "Admin")
            {
                return RedirectToPage("/Login");
            }

            var kitchens = await _ckStaffService.GetAllCentralKitchensAsync();
            CentralKitchens = kitchens.Select(ck => new SelectListItem
            {
                Value = ck.Id.ToString(),
                Text = ck.Name
            }).ToList();

            int ckId = CentralKitchenId ?? kitchens.FirstOrDefault()?.Id ?? 0;
            if (ckId == 0) return Page();

            CentralKitchenId = ckId;

            var allOrders = await _orderService.GetOrdersByCentralKitchenIdAsync(ckId);
            PendingOrders = allOrders.Count(o => o.Status == "Pending");
            PreparingOrders = allOrders.Count(o => o.Status == "Approved" || o.Status == "Producing");
            DeliveringOrders = allOrders.Count(o => o.Status == "Delivering");

            var demand = await _ckStaffService.GetAggregatedDemandAsync(ckId);
            TopDemand = demand.Where(d => d.ShortageQuantity > 0).Take(5).ToList();

            var inProgress = await _productionService.GetInProgressProductionsAsync(ckId);
            InProgressProductions = inProgress.Select(p => new InProgressProductionItem
            {
                ProductionId = p.ProductionId,
                RecipeName = p.Recipe?.Name ?? "",
                ProductName = p.Recipe?.Product?.Name ?? "",
                PlannedQuantity = p.PlannedQuantity,
                StartTime = p.StartTime,
                KitchenName = p.CentralKitchen?.Name ?? ""
            }).ToList();

            return Page();
        }
    }
}
