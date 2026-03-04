using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessLogicLayer_FranLink.DTOs;
using BusinessLogicLayer_FranLink.Services;
using DataAccessLayer_FranLink.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PresentationLayer_FranLink.Pages.Admin.Users
{
    public class CreateModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly IStoreService _storeService;

        public CreateModel(IUserService userService, IStoreService storeService)
        {
            _userService = userService;
            _storeService = storeService;
        }

        [BindProperty]
        public CreateUserDto Input { get; set; }

        public List<SelectListItem> RoleOptions { get; set; } = new();
        public List<SelectListItem> StoreOptions { get; set; } = new();
        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin") return RedirectToPage("/Login");

            await LoadDropdowns();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin") return RedirectToPage("/Login");

            if (!ModelState.IsValid)
            {
                await LoadDropdowns();
                return Page();
            }

            try
            {
                await _userService.CreateUserAsync(Input);
                return RedirectToPage("Index");
            }
            catch (InvalidOperationException ex)
            {
                ErrorMessage = ex.Message;
                await LoadDropdowns();
                return Page();
            }
        }

        private async Task LoadDropdowns()
        {
            var roles = await _userService.GetAllRolesAsync();
            RoleOptions = roles.Select(r => new SelectListItem(r.RoleName, r.RoleId.ToString())).ToList();

            var stores = await _storeService.GetAllStoresAsync();
            StoreOptions = stores
                .Where(s => s.IsActive)
                .Select(s => new SelectListItem(
                    s.Name + (s.IsCentralKitchen ? " (Bếp TT)" : " (Cửa hàng)"),
                    s.Id.ToString()))
                .ToList();
            StoreOptions.Insert(0, new SelectListItem("— Không thuộc cửa hàng —", ""));
        }
    }
}
