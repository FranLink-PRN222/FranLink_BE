using System.Threading.Tasks;
using BusinessLogicLayer_FranLink.DTOs;
using BusinessLogicLayer_FranLink.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PresentationLayer_FranLink.Pages.Admin.Stores
{
    public class CreateModel : PageModel
    {
        private readonly IStoreService _storeService;

        public CreateModel(IStoreService storeService)
        {
            _storeService = storeService;
        }

        [BindProperty]
        public CreateStoreDto Input { get; set; }

        public IActionResult OnGet()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin") return RedirectToPage("/Login");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin") return RedirectToPage("/Login");

            if (!ModelState.IsValid) return Page();

            await _storeService.CreateStoreAsync(Input);
            return RedirectToPage("Index");
        }
    }
}
