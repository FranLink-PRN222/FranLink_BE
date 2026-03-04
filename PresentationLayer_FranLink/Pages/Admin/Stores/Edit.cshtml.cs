using System.Threading.Tasks;
using BusinessLogicLayer_FranLink.DTOs;
using BusinessLogicLayer_FranLink.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PresentationLayer_FranLink.Pages.Admin.Stores
{
    public class EditModel : PageModel
    {
        private readonly IStoreService _storeService;

        public EditModel(IStoreService storeService)
        {
            _storeService = storeService;
        }

        [BindProperty]
        public UpdateStoreDto Input { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin") return RedirectToPage("/Login");

            var store = await _storeService.GetStoreByIdAsync(id);
            if (store == null) return NotFound();

            Input = new UpdateStoreDto
            {
                Id = store.Id,
                Name = store.Name,
                Address = store.Address,
                Phone = store.Phone,
                IsCentralKitchen = store.IsCentralKitchen,
                IsActive = store.IsActive
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin") return RedirectToPage("/Login");

            if (!ModelState.IsValid) return Page();

            await _storeService.UpdateStoreAsync(Input);
            return RedirectToPage("Index");
        }
    }
}
