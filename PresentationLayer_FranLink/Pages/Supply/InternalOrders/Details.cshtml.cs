using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessLogicLayer_FranLink.Services;
using DataAccessLayer_FranLink.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PresentationLayer_FranLink.Pages.Supply.InternalOrders
{
    public class DetailsModel : PageModel
    {
        private readonly IInternalOrderService _orderService;
        private readonly FranLinkContext _context;

        public DetailsModel(IInternalOrderService orderService, FranLinkContext context)
        {
            _orderService = orderService;
            _context = context;
        }

        public InternalOrder Order { get; set; }
        public IList<ItemStockViewModel> Items { get; set; } = new List<ItemStockViewModel>();
        public bool HasSufficientInventory { get; set; }

        public class ItemStockViewModel
        {
            public string ProductName { get; set; } = string.Empty;
            public int RequestedQuantity { get; set; }
            public int AvailableQuantity { get; set; }
            public bool IsSufficient => AvailableQuantity >= RequestedQuantity;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "SupplyCoordinator" && role != "Supply Coordinator")
            {
                return RedirectToPage("/Login");
            }

            Order = await _orderService.GetOrderByIdAsync(id);
            if (Order == null)
            {
                return NotFound();
            }

            await LoadItemStocksAsync();
            return Page();
        }

        private async Task LoadItemStocksAsync()
        {
            Items.Clear();
            HasSufficientInventory = true;

            if (!Order.CentralKitchenId.HasValue || Order.Items == null || !Order.Items.Any())
            {
                HasSufficientInventory = false;
                return;
            }

            var centralKitchenId = Order.CentralKitchenId.Value;

            foreach (var item in Order.Items)
            {
                var available = _context.Inventories
                    .Where(i => i.CentralKitchenId == centralKitchenId && i.ProductId == item.ProductId)
                    .Sum(i => i.Quantity);

                var vm = new ItemStockViewModel
                {
                    ProductName = item.Product?.Name ?? $"Product ID {item.ProductId}",
                    RequestedQuantity = item.Quantity,
                    AvailableQuantity = available
                };

                if (!vm.IsSufficient)
                {
                    HasSufficientInventory = false;
                }

                Items.Add(vm);
            }
        }

        public async Task<IActionResult> OnPostApproveAsync(int id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "SupplyCoordinator" && role != "Supply Coordinator")
            {
                return RedirectToPage("/Login");
            }

            try
            {
                await _orderService.ApproveOrderForSupplyAsync(id);
                TempData["SuccessMessage"] = "Order approved successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToPage("Index");
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

            return RedirectToPage("Index");
        }
    }
}

