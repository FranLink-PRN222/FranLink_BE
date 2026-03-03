using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using DataAccessLayer_FranLink.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace PresentationLayer_FranLink.Pages
{
    public class LoginModel : PageModel
    {
        private readonly FranLinkContext _context;

        public LoginModel(FranLinkContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required]
            public string Username { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        }

        public void OnGet(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (ModelState.IsValid)
            {
                var user = await _context.Users
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.Username == Input.Username);

                if (user != null)
                {
                    // TODO: Implement proper password hashing verification here.
                    // Currently using plain text comparison as no hashing utility was found.
                    if (user.PasswordHash == Input.Password) 
                    {
                        HttpContext.Session.SetString("UserId", user.UserId.ToString());
                        HttpContext.Session.SetString("Username", user.Username);

                        // Assuming user has at least one role, or taking the first one for simplicity in Session
                        var role = user.UserRoles.FirstOrDefault()?.Role?.RoleName;
                        if (!string.IsNullOrEmpty(role))
                        {
                            HttpContext.Session.SetString("Role", role);
                        }

                        return LocalRedirect(returnUrl);
                    }
                }

                ModelState.AddModelError(string.Empty, "Invalid username or password.");
            }

            return Page();
        }
    }
}
