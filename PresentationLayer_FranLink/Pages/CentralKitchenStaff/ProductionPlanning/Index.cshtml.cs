using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessLogicLayer_FranLink.DTOs;
using BusinessLogicLayer_FranLink.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PresentationLayer_FranLink.Pages.CentralKitchenStaff.ProductionPlanning
{
    public class IndexModel : PageModel
    {
        private readonly ICentralKitchenStaffService _ckStaffService;

        public IndexModel(ICentralKitchenStaffService ckStaffService)
        {
            _ckStaffService = ckStaffService;
        }

        public List<AggregatedDemandItem> Demand { get; set; } = new();
        public List<SelectListItem> CentralKitchens { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int CentralKitchenId { get; set; }

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

            Demand = await _ckStaffService.GetAggregatedDemandAsync(CentralKitchenId);

            return Page();
        }
    }
}
