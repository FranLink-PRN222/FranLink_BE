using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessLogicLayer_FranLink.Services;
using DataAccessLayer_FranLink.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PresentationLayer_FranLink.Pages.Admin.Stores
{
    public class IndexModel : PageModel
    {
        private readonly IStoreService _storeService;

        public IndexModel(IStoreService storeService)
        {
            _storeService = storeService;
        }

        public IList<FranchiseStore> Stores { get; set; } = new List<FranchiseStore>();

        public async Task<IActionResult> OnGetAsync()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin") return RedirectToPage("/Login");

            Stores = await _storeService.GetAllStoresAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostToggleAsync(int storeId)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin") return RedirectToPage("/Login");

            await _storeService.ToggleStoreActiveAsync(storeId);
            return RedirectToPage();
        }
    }
}
