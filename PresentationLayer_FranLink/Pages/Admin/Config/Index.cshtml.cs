using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessLogicLayer_FranLink.Services;
using DataAccessLayer_FranLink.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PresentationLayer_FranLink.Pages.Admin.Config
{
    public class IndexModel : PageModel
    {
        private readonly ISystemConfigService _configService;

        public IndexModel(ISystemConfigService configService)
        {
            _configService = configService;
        }

        public IList<SystemConfig> Configs { get; set; } = new List<SystemConfig>();

        [BindProperty]
        public string NewKey { get; set; }
        [BindProperty]
        public string NewValue { get; set; }
        [BindProperty]
        public string NewDescription { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin") return RedirectToPage("/Login");

            Configs = await _configService.GetAllConfigsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAddAsync()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin") return RedirectToPage("/Login");

            if (!string.IsNullOrWhiteSpace(NewKey) && !string.IsNullOrWhiteSpace(NewValue))
            {
                await _configService.SetConfigAsync(NewKey.Trim(), NewValue.Trim(), NewDescription?.Trim() ?? "");
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateAsync(string key, string value, string description)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin") return RedirectToPage("/Login");

            if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(value))
            {
                await _configService.SetConfigAsync(key, value, description ?? "");
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin") return RedirectToPage("/Login");

            await _configService.DeleteConfigAsync(id);
            return RedirectToPage();
        }
    }
}
