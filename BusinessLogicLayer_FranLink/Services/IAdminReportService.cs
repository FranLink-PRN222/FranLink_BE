using BusinessLogicLayer_FranLink.DTOs;

namespace BusinessLogicLayer_FranLink.Services
{
    public interface IAdminReportService
    {
        Task<AdminDashboardDto> GetDashboardAsync();
    }
}
