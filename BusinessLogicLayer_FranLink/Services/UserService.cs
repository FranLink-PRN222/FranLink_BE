using DataAccessLayer_FranLink.Models;
using BusinessLogicLayer_FranLink.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogicLayer_FranLink.Services
{
    public class UserService : IUserService
    {
        private readonly FranLinkContext _context;

        public UserService(FranLinkContext context)
        {
            _context = context;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Include(u => u.FranchiseStore)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        public async Task<User> GetUserByIdAsync(Guid id)
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Include(u => u.FranchiseStore)
                .FirstOrDefaultAsync(u => u.UserId == id);
        }

        public async Task<User> CreateUserAsync(CreateUserDto dto)
        {
            // Check for duplicate username
            var exists = await _context.Users.AnyAsync(u => u.Username == dto.Username);
            if (exists)
                throw new InvalidOperationException($"Username '{dto.Username}' already exists.");

            var user = new User
            {
                UserId = Guid.NewGuid(),
                Username = dto.Username,
                PasswordHash = dto.Password, // TODO: Hash password
                Email = dto.Email,
                FullName = dto.FullName,
                Phone = dto.Phone,
                FranchiseStoreId = dto.FranchiseStoreId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);

            // Assign role
            _context.UserRoles.Add(new UserRole
            {
                UserId = user.UserId,
                RoleId = dto.RoleId
            });

            await _context.SaveChangesAsync();
            return user;
        }

        public async Task UpdateUserAsync(UpdateUserDto dto)
        {
            var user = await _context.Users.FindAsync(dto.UserId);
            if (user == null)
                throw new InvalidOperationException("User not found.");

            user.Email = dto.Email;
            user.FullName = dto.FullName;
            user.Phone = dto.Phone;
            user.FranchiseStoreId = dto.FranchiseStoreId;
            user.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();
        }

        public async Task ToggleUserActiveAsync(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new InvalidOperationException("User not found.");

            user.IsActive = !user.IsActive;
            await _context.SaveChangesAsync();
        }

        public async Task AssignRoleAsync(Guid userId, int roleId)
        {
            var exists = await _context.UserRoles
                .AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
            if (exists) return;

            _context.UserRoles.Add(new UserRole
            {
                UserId = userId,
                RoleId = roleId
            });
            await _context.SaveChangesAsync();
        }

        public async Task RemoveRoleAsync(Guid userId, int roleId)
        {
            var userRole = await _context.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
            if (userRole == null) return;

            _context.UserRoles.Remove(userRole);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Role>> GetAllRolesAsync()
        {
            return await _context.Roles.OrderBy(r => r.RoleId).ToListAsync();
        }
    }
}
