using System;
using System.Collections.Generic;
using System.Linq;
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
    public class EditModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly IStoreService _storeService;

        public EditModel(IUserService userService, IStoreService storeService)
        {
            _userService = userService;
            _storeService = storeService;
        }

        [BindProperty]
        public UpdateUserDto Input { get; set; }

        public string Username { get; set; }
        public List<Role> AllRoles { get; set; } = new();
        public List<int> CurrentRoleIds { get; set; } = new();
        public List<SelectListItem> StoreOptions { get; set; } = new();
        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin") return RedirectToPage("/Login");

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound();

            Username = user.Username;
            Input = new UpdateUserDto
            {
                UserId = user.UserId,
                Email = user.Email,
                FullName = user.FullName,
                Phone = user.Phone,
                FranchiseStoreId = user.FranchiseStoreId,
                IsActive = user.IsActive
            };

            CurrentRoleIds = user.UserRoles?.Select(ur => ur.RoleId).ToList() ?? new();
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
                await _userService.UpdateUserAsync(Input);
                return RedirectToPage("Index");
            }
            catch (InvalidOperationException ex)
            {
                ErrorMessage = ex.Message;
                await LoadDropdowns();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAssignRoleAsync(Guid userId, int roleId)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin") return RedirectToPage("/Login");

            await _userService.AssignRoleAsync(userId, roleId);
            return RedirectToPage(new { id = userId });
        }

        public async Task<IActionResult> OnPostRemoveRoleAsync(Guid userId, int roleId)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin") return RedirectToPage("/Login");

            await _userService.RemoveRoleAsync(userId, roleId);
            return RedirectToPage(new { id = userId });
        }

        private async Task LoadDropdowns()
        {
            AllRoles = await _userService.GetAllRolesAsync();
            var user = await _userService.GetUserByIdAsync(Input.UserId);
            CurrentRoleIds = user?.UserRoles?.Select(ur => ur.RoleId).ToList() ?? new();

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
