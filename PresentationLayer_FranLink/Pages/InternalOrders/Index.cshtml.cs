using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessLogicLayer_FranLink.Services;
using DataAccessLayer_FranLink.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PresentationLayer_FranLink.Pages.InternalOrders
{
    public class IndexModel : PageModel
    {
        private readonly IInternalOrderService _orderService;

        public IndexModel(IInternalOrderService orderService)
        {
            _orderService = orderService;
        }

        public IList<OrderViewModel> Orders { get; set; } = new List<OrderViewModel>();

        public async Task OnGetAsync()
        {
            // Hardcoded Store ID for demonstration. In real app, get from User Claims.
            // Assuming Store 1 is the logged-in staff's store.
            int storeId = 1; 

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
