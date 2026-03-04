using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessLogicLayer_FranLink.DTOs;
using DataAccessLayer_FranLink.Models;

namespace BusinessLogicLayer_FranLink.Services
{
    public interface ICentralKitchenStaffService
    {
        /// <summary>
        /// Lấy tổng hợp nhu cầu từ các đơn hàng Pending và Approved
        /// </summary>
        Task<List<AggregatedDemandItem>> GetAggregatedDemandAsync(int centralKitchenId);

        /// <summary>
        /// Lấy danh sách nguyên liệu với thông tin hạn sử dụng và lô
        /// </summary>
        Task<List<MaterialWithExpiryDto>> GetMaterialsWithExpiryAsync(int centralKitchenId, bool? expiringOnly = null);

        Task<List<CentralKitchen>> GetAllCentralKitchensAsync();
    }
}
