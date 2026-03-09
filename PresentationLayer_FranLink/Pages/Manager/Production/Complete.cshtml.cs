using BusinessLogicLayer_FranLink.DTOs;
using BusinessLogicLayer_FranLink.Services;
using DataAccessLayer_FranLink.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PresentationLayer_FranLink.Pages.Manager.Production
{
    public class CompleteModel : PageModel
    {
        private readonly IProductionService _productionService;

        public CompleteModel(IProductionService productionService)
        {
            _productionService = productionService;
        }

        public ProductionRecord Production { get; set; } = null!;

        [BindProperty]
        public CompleteProductionDto Input { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Manager")
            {
                return RedirectToPage("/Login");
            }

            var production = await _productionService.GetByIdAsync(Id);
            if (production == null)
            {
                return NotFound();
            }

            if (production.Status != "InProgress")
            {
                TempData["Error"] = "Cannot complete a production that is not in progress.";
                return RedirectToPage("Index");
            }

            Production = production;
            Input.ActualQuantity = production.PlannedQuantity;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Manager")
            {
                return RedirectToPage("/Login");
            }

            var result = await _productionService.CompleteProductionAsync(Id, Input);
            if (result == null)
            {
                TempData["Error"] = "Failed to complete production.";
                return RedirectToPage("Index");
            }

            TempData["Success"] = $"Production #{Id} completed successfully! Produced {Input.ActualQuantity} units.";
            return RedirectToPage("Index");
        }
    }
}
