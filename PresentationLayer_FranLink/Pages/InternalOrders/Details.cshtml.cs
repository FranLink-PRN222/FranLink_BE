using BusinessLogicLayer_FranLink.Services;
using DataAccessLayer_FranLink.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace PresentationLayer_FranLink.Pages.InternalOrders
{
    public class DetailsModel : PageModel
    {
        private readonly IInternalOrderService _orderService;

        public DetailsModel(IInternalOrderService orderService)
        {
            _orderService = orderService;
        }

        public InternalOrder Order { get; set; }

        [BindProperty]
        public FeedbackViewModel Feedback { get; set; }

        public bool CanConfirmReceived { get; set; }
        public bool CanGiveFeedback { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Order = await _orderService.GetOrderByIdAsync(id);

            if (Order == null)
            {
                return NotFound();
            }

            // Logic for Confirm Received Button
            // Visible if Delivery Status is Completed AND Order Status is NOT Completed
            CanConfirmReceived = Order.Delivery != null &&
                                 Order.Delivery.DeliveryStatus == "Completed" &&
                                 Order.Status != "Completed";

            // Logic for Feedback
            // Visible if Order Status is Completed
            CanGiveFeedback = Order.Status == "Completed";

            return Page();
        }

        public async Task<IActionResult> OnPostConfirmReceivedAsync(int id)
        {
            try
            {
                await _orderService.ConfirmOrderReceivedAsync(id);
                TempData["SuccessMessage"] = "Order confirmed received and inventory updated.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostSubmitFeedbackAsync(int id)
        {
            if (!ModelState.IsValid)
            {
                // Reload order to display page correctly
                Order = await _orderService.GetOrderByIdAsync(id);
                CanConfirmReceived = false;
                CanGiveFeedback = true;
                return Page();
            }

            // Get logged-in user from Session
            var userIdString = HttpContext.Session.GetString("UserId");
            
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            {
                ModelState.AddModelError("", "You must be logged in to submit feedback. Please login first.");
                Order = await _orderService.GetOrderByIdAsync(id);
                CanConfirmReceived = false;
                CanGiveFeedback = true;
                return Page();
            }

            var feedback = new QualityFeedback
            {
                QualityFeedbackId = Guid.NewGuid(),
                UserId = userId, 
                ProductId = Feedback.ProductId, 
                Rating = Feedback.Rating,
                Comment = Feedback.Comment,
                CreatedAt = DateTime.UtcNow
            };

            await _orderService.AddFeedbackAsync(feedback); 
            TempData["SuccessMessage"] = "Feedback submitted successfully.";

            return RedirectToPage(new { id });
        }

        public class FeedbackViewModel
        {
            [Required]
            public int ProductId { get; set; }
            [Range(1, 5)]
            public int Rating { get; set; }
            public string Comment { get; set; }
        }
    }
}
