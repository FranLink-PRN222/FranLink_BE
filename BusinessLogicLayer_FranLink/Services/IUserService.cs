using DataAccessLayer_FranLink.Models;
using BusinessLogicLayer_FranLink.DTOs;

namespace BusinessLogicLayer_FranLink.Services
{
    public interface IUserService
    {
        Task<List<User>> GetAllUsersAsync();
        Task<User> GetUserByIdAsync(Guid id);
        Task<User> CreateUserAsync(CreateUserDto dto);
        Task UpdateUserAsync(UpdateUserDto dto);
        Task ToggleUserActiveAsync(Guid userId);
        Task AssignRoleAsync(Guid userId, int roleId);
        Task RemoveRoleAsync(Guid userId, int roleId);
        Task<List<Role>> GetAllRolesAsync();
    }
}
