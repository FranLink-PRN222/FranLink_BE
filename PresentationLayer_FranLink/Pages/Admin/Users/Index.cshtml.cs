using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessLogicLayer_FranLink.Services;
using DataAccessLayer_FranLink.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PresentationLayer_FranLink.Pages.Admin.Users
{
    public class IndexModel : PageModel
    {
        private readonly IUserService _userService;

        public IndexModel(IUserService userService)
        {
            _userService = userService;
        }

        public IList<User> Users { get; set; } = new List<User>();

        public async Task<IActionResult> OnGetAsync()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin") return RedirectToPage("/Login");

            Users = await _userService.GetAllUsersAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostToggleAsync(Guid userId)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin") return RedirectToPage("/Login");

            await _userService.ToggleUserActiveAsync(userId);
            return RedirectToPage();
        }
    }
}
