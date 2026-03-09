using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessLogicLayer_FranLink.Services;
using DataAccessLayer_FranLink.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PresentationLayer_FranLink.Pages.CentralKitchenStaff.Orders
{
    public class IndexModel : PageModel
    {
        private readonly IInternalOrderService _orderService;
        private readonly ICentralKitchenStaffService _ckStaffService;

        public IndexModel(IInternalOrderService orderService, ICentralKitchenStaffService ckStaffService)
        {
            _orderService = orderService;
            _ckStaffService = ckStaffService;
        }

        public List<OrderRow> Orders { get; set; } = new();
        public List<SelectListItem> CentralKitchens { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int CentralKitchenId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? StatusFilter { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "CentralKitchenStaff" && role != "Central Kitchen Staff")
            {
                return RedirectToPage("/Login");
            }

            var kitchens = await _ckStaffService.GetAllCentralKitchensAsync();
            CentralKitchens = kitchens.Select(ck => new SelectListItem
            {
                Value = ck.Id.ToString(),
                Text = ck.Name,
                Selected = ck.Id == CentralKitchenId
            }).ToList();

            if (CentralKitchenId == 0 && kitchens.Any())
            {
                CentralKitchenId = kitchens.First().Id;
            }

            if (CentralKitchenId == 0)
            {
                return Page();
            }

            var list = await _orderService.GetOrdersByCentralKitchenIdAsync(CentralKitchenId, StatusFilter);

            Orders = list.Select(o => new OrderRow
            {
                Id = o.Id,
                OrderDate = o.OrderDate,
                StoreName = o.FranchiseStore?.Name ?? "Unknown",
                Status = o.Status,
                DeliveryStatus = o.Delivery?.DeliveryStatus ?? "-",
                TotalItems = o.Items?.Sum(i => i.Quantity) ?? 0,
                TotalValue = o.Items?.Sum(i => i.Quantity * i.UnitPrice) ?? 0
            }).ToList();

            return Page();
        }

        public class OrderRow
        {
            public int Id { get; set; }
            public System.DateTime OrderDate { get; set; }
            public string StoreName { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public string DeliveryStatus { get; set; } = string.Empty;
            public int TotalItems { get; set; }
            public decimal TotalValue { get; set; }
        }
    }
}
