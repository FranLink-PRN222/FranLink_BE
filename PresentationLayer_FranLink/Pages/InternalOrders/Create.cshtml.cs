using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessLogicLayer_FranLink.DTOs;
using BusinessLogicLayer_FranLink.Services;
using DataAccessLayer_FranLink.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PresentationLayer_FranLink.Pages.InternalOrders
{
    public class CreateModel : PageModel
    {
        private readonly IInternalOrderService _orderService;
        private readonly FranLinkContext _context;

        public CreateModel(IInternalOrderService orderService, FranLinkContext context)
        {
            _orderService = orderService;
            _context = context;
        }

        [BindProperty]
        public CreateInternalOrderDto Order { get; set; } = new();

        public SelectList StoreList { get; set; }
        public SelectList ProductList { get; set; }

        public void OnGet()
        {
            // Populate Dropdowns
            StoreList = new SelectList(_context.FranchiseStores.ToList(), "Id", "Name");
            ProductList = new SelectList(_context.Products.ToList(), "Id", "Name");

            // Initialize minimal Order items list for UI (e.g. 1 item)
            Order.Items.Add(new CreateInternalOrderItemDto { Quantity = 1 });
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                StoreList = new SelectList(_context.FranchiseStores.ToList(), "Id", "Name");
                ProductList = new SelectList(_context.Products.ToList(), "Id", "Name");
                return Page();
            }

            // In a real app, strict user context (UserId) would come from User.Identity. 
            // Here we might mock it or take it from form if admin. 
            // For now, let's hardcode or pick first user if not provided/auth logic missing.
            // Using a dummy user ID for demonstration if Order.UserId is empty.
            if (Order.UserId == Guid.Empty)
            {
                var user = _context.Users.FirstOrDefault();
                if (user != null)
                    Order.UserId = user.UserId;
                else
                    Order.UserId = Guid.NewGuid(); // Fallback
            }

            try 
            {
                await _orderService.CreateOrderAsync(Order);
                return RedirectToPage("/Index"); // Or Success page
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                StoreList = new SelectList(_context.FranchiseStores.ToList(), "Id", "Name");
                ProductList = new SelectList(_context.Products.ToList(), "Id", "Name");
                return Page();
            }
        }
    }
}
