using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessLogicLayer_FranLink.Services;
using DataAccessLayer_FranLink.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PresentationLayer_FranLink.Pages.Supply.InternalOrders
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

        public IList<InternalOrder> Orders { get; set; } = new List<InternalOrder>();
        // Đánh dấu mỗi order có đủ tồn kho tại bếp hay không
        public Dictionary<int, bool> HasSufficientInventory { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "SupplyCoordinator" && role != "Supply Coordinator")
            {
                return RedirectToPage("/Login");
            }

            Orders = await _orderService.GetOrdersForSupplyAsync();

            // Tính xem từng order có đủ hàng tại bếp được gán hay không (để hiển thị UI)
            HasSufficientInventory.Clear();
            foreach (var order in Orders)
            {
                var ok = true;

                if (!order.CentralKitchenId.HasValue || order.Items == null || !order.Items.Any())
                {
                    ok = false;
                }
                else
                {
                    foreach (var item in order.Items)
                    {
                        var totalQty = _context.Inventories
                            .Where(i => i.CentralKitchenId == order.CentralKitchenId && i.ProductId == item.ProductId)
                            .Sum(i => i.Quantity);

                        if (totalQty < item.Quantity)
                        {
                            ok = false;
                            break;
                        }
                    }
                }

                HasSufficientInventory[order.Id] = ok;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostCancelAsync(int id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "SupplyCoordinator" && role != "Supply Coordinator")
            {
                return RedirectToPage("/Login");
            }

            try
            {
                await _orderService.CancelOrderAsync(id);
                TempData["SuccessMessage"] = "Order cancelled successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostStartProductionAsync(int id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "SupplyCoordinator" && role != "Supply Coordinator")
            {
                return RedirectToPage("/Login");
            }

            try
            {
                await _orderService.StartProductionAsync(id);
                TempData["SuccessMessage"] = "Production started.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostMarkReadyAsync(int id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "SupplyCoordinator" && role != "Supply Coordinator")
            {
                return RedirectToPage("/Login");
            }

            try
            {
                await _orderService.MarkProductionReadyAsync(id);
                TempData["SuccessMessage"] = "Order marked as ready.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostStartDeliveryAsync(int id, DateTime? scheduledDate)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "SupplyCoordinator" && role != "Supply Coordinator")
            {
                return RedirectToPage("/Login");
            }

            try
            {
                await _orderService.StartOrAdvanceDeliveryAsync(id, scheduledDate);
                TempData["SuccessMessage"] = "Delivery status updated.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostMarkDeliveredAsync(int id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "SupplyCoordinator" && role != "Supply Coordinator")
            {
                return RedirectToPage("/Login");
            }

            try
            {
                await _orderService.MarkDeliveredAsync(id);
                TempData["SuccessMessage"] = "Order marked as delivered.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToPage();
        }
    }
}

