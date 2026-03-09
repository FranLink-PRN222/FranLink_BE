using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessLogicLayer_FranLink.Services;
using DataAccessLayer_FranLink.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PresentationLayer_FranLink.Pages.InternalOrders
{
    public class IndexModel : PageModel
    {
        private readonly IInternalOrderService _orderService;
        private readonly FranLinkContext _context;

        public IndexModel(IInternalOrderService orderService, FranLinkContext context)
        {
            _orderService = orderService;
            _context = context;
        }

        public IList<OrderViewModel> Orders { get; set; } = new List<OrderViewModel>();

        public async Task OnGetAsync()
        {
            // Default fallback store (for safety if session/user mapping is missing)
            int storeId = 1;

            // Try to resolve store based on logged-in user
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (!string.IsNullOrEmpty(userIdStr) && Guid.TryParse(userIdStr, out var userId))
            {
                var user = await _context.Users.FindAsync(userId);
                if (user?.FranchiseStoreId != null)
                {
                    storeId = user.FranchiseStoreId.Value;
                }
            }

            var orders = await _orderService.GetOrdersByStoreIdAsync(storeId);

            Orders = orders.Select(o => new OrderViewModel
            {
                Id = o.Id,
                OrderDate = o.OrderDate,
                Status = GetStatus(o),
                TotalItems = o.Items?.Sum(i => i.Quantity) ?? 0
            }).ToList();
        }

        private string GetStatus(InternalOrder order)
        {
            if (order.Status == "Completed") return "Completed";
            if (order.Delivery != null && order.Delivery.DeliveryStatus == "Delivering") return "Delivering";
            return order.Status; // Pending, Producing, etc.
        }

        public class OrderViewModel
        {
            public int Id { get; set; }
            public DateTime OrderDate { get; set; }
            public string Status { get; set; }
            public int TotalItems { get; set; }
        }
    }
}
