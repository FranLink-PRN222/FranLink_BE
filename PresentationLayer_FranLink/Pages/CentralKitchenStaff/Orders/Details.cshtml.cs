using System.Linq;
using System.Threading.Tasks;
using BusinessLogicLayer_FranLink.Services;
using DataAccessLayer_FranLink.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PresentationLayer_FranLink.Pages.CentralKitchenStaff.Orders
{
    public class DetailsModel : PageModel
    {
        private readonly IInternalOrderService _orderService;

        public DetailsModel(IInternalOrderService orderService)
        {
            _orderService = orderService;
        }

        public InternalOrder Order { get; set; } = null!;
        public string? Message { get; set; }
        public bool IsSuccess { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "CentralKitchenStaff" && role != "Central Kitchen Staff" && role != "Manager" && role != "Admin")
            {
                return RedirectToPage("/Login");
            }

            Order = await _orderService.GetOrderByIdAsync(id);
            if (Order == null)
            {
                return NotFound();
            }

            if (TempData["Message"] is string msg)
            {
                Message = msg;
                IsSuccess = TempData["IsSuccess"] as bool? ?? true;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostApproveAsync(int id)
        {
            try
            {
                await _orderService.ApproveOrderAsync(id);
                TempData["Message"] = "Order approved successfully.";
                TempData["IsSuccess"] = true;
            }
            catch (System.Exception ex)
            {
                TempData["Message"] = ex.Message;
                TempData["IsSuccess"] = false;
            }
            return RedirectToPage("Details", new { id });
        }

        public async Task<IActionResult> OnPostRejectAsync(int id)
        {
            try
            {
                await _orderService.RejectOrderAsync(id);
                TempData["Message"] = "Order rejected.";
                TempData["IsSuccess"] = true;
            }
            catch (System.Exception ex)
            {
                TempData["Message"] = ex.Message;
                TempData["IsSuccess"] = false;
            }
            return RedirectToPage("Details", new { id });
        }

        public async Task<IActionResult> OnPostStartPreparingAsync(int id)
        {
            try
            {
                await _orderService.StartPreparingOrderAsync(id);
                TempData["Message"] = "Order is now in production (Producing).";
                TempData["IsSuccess"] = true;
            }
            catch (System.Exception ex)
            {
                TempData["Message"] = ex.Message;
                TempData["IsSuccess"] = false;
            }
            return RedirectToPage("Details", new { id });
        }

        public async Task<IActionResult> OnPostMarkReadyAsync(int id)
        {
            try
            {
                // In service: Producing -> Ready
                await _orderService.MarkProductionReadyAsync(id);
                TempData["Message"] = "Order marked as Ready for pickup/delivery.";
                TempData["IsSuccess"] = true;
            }
            catch (System.Exception ex)
            {
                TempData["Message"] = ex.Message;
                TempData["IsSuccess"] = false;
            }
            return RedirectToPage("Details", new { id });
        }

        public async Task<IActionResult> OnPostMarkDeliveringAsync(int id)
        {
            try
            {
                // In service: Ready -> DeliveryStatus=InTransit
                await _orderService.StartOrAdvanceDeliveryAsync(id);
                TempData["Message"] = "Order has been dispatched (In Transit).";
                TempData["IsSuccess"] = true;
            }
            catch (System.Exception ex)
            {
                TempData["Message"] = ex.Message;
                TempData["IsSuccess"] = false;
            }
            return RedirectToPage("Details", new { id });
        }

        public async Task<IActionResult> OnPostMarkDeliveryCompletedAsync(int id)
        {
            try
            {
                // In service: InTransit -> Delivered (Arrived at store)
                await _orderService.MarkDeliveredAsync(id);
                TempData["Message"] = "Order marked as Delivered (Arrived at store). Waiting for store confirmation.";
                TempData["IsSuccess"] = true;
            }
            catch (System.Exception ex)
            {
                TempData["Message"] = ex.Message;
                TempData["IsSuccess"] = false;
            }
            return RedirectToPage("Details", new { id });
        }
    }
}
