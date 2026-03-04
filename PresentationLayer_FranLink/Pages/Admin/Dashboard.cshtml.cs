using System.Threading.Tasks;
using BusinessLogicLayer_FranLink.DTOs;
using BusinessLogicLayer_FranLink.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PresentationLayer_FranLink.Pages.Admin
{
    public class DashboardModel : PageModel
    {
        private readonly IAdminReportService _reportService;

        public DashboardModel(IAdminReportService reportService)
        {
            _reportService = reportService;
        }

        public AdminDashboardDto Dashboard { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin") return RedirectToPage("/Login");

            Dashboard = await _reportService.GetDashboardAsync();
            return Page();
        }
    }
}
