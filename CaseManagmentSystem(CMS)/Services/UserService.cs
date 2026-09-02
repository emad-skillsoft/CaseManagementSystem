using CaseManagementSystem.Constants;
using CaseManagementSystem.Models;
using CaseManagementSystem.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CaseManagementSystem.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public Task<List<ApplicationUser>> GetUsersAsync()
        {
            return _userManager.Users
                .OrderBy(x => x.FullName)
                .ToListAsync();
        }

        // Session 5 - User list with roles
        public async Task<List<UserListItemViewModel>> GetUserListAsync()
        {
            var users = await _userManager.Users
                .OrderBy(x => x.FullName)
                .ToListAsync();

            var result = new List<UserListItemViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                result.Add(new UserListItemViewModel
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    EmployeeNumber = user.EmployeeNumber,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    Role = roles.FirstOrDefault() ?? string.Empty
                });
            }

            return result;
        }

        public async Task<ApplicationUser?> GetExpertByEmployeeNumberAsync(
            string employeeNumber)
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(
                    x => x.EmployeeNumber == employeeNumber);

            if (user == null)
                return null;

            return await _userManager.IsInRoleAsync(
                user,
                RoleNames.Expert)
                ? user
                : null;
        }

        public async Task<IdentityResult> CreateUserAsync(
            CreateUserViewModel model)
        {
            if (model.Role != RoleNames.Supervisor &&
                model.Role != RoleNames.Expert)
            {
                return IdentityResult.Failed(
                    new IdentityError
                    {
                        Description = "Invalid role."
                    });
            }

            var employeeExists = await _userManager.Users
                .AnyAsync(
                    x => x.EmployeeNumber == model.EmployeeNumber);

            if (employeeExists)
            {
                return IdentityResult.Failed(
                    new IdentityError
                    {
                        Description =
                            "Employee Number already exists."
                    });
            }

            var user = new ApplicationUser
            {
                FullName = model.FullName.Trim(),
                EmployeeNumber = model.EmployeeNumber.Trim(),
                UserName = model.UserName.Trim(),
                Email = model.Email.Trim(),
                EmailConfirmed = true
            };

            var createResult =
                await _userManager.CreateAsync(
                    user,
                    model.Password);

            if (!createResult.Succeeded)
                return createResult;

            var roleResult =
                await _userManager.AddToRoleAsync(
                    user,
                    model.Role);

            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                return roleResult;
            }

            return IdentityResult.Success;
        }

        public async Task<EditUserViewModel?> GetUserForEditAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return null;

            var roles = await _userManager.GetRolesAsync(user);

            return new EditUserViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                EmployeeNumber = user.EmployeeNumber,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Role = roles.FirstOrDefault() ?? string.Empty
            };
        }

        public async Task<IdentityResult> UpdateUserAsync(
            EditUserViewModel model)
        {
            if (model.Role != RoleNames.Supervisor &&
                model.Role != RoleNames.Expert)
            {
                return IdentityResult.Failed(
                    new IdentityError
                    {
                        Description = "Invalid role."
                    });
            }

            var user = await _userManager.FindByIdAsync(model.Id);

            if (user == null)
            {
                return IdentityResult.Failed(
                    new IdentityError
                    {
                        Description = "User not found."
                    });
            }

            var duplicateEmployee =
                await _userManager.Users.AnyAsync(x =>
                    x.EmployeeNumber == model.EmployeeNumber &&
                    x.Id != model.Id);

            if (duplicateEmployee)
            {
                return IdentityResult.Failed(
                    new IdentityError
                    {
                        Description =
                            "Employee Number already exists."
                    });
            }

            user.FullName = model.FullName.Trim();
            user.EmployeeNumber = model.EmployeeNumber.Trim();
            user.UserName = model.UserName.Trim();
            user.Email = model.Email.Trim();

            var updateResult =
                await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
                return updateResult;

            var oldRoles =
                await _userManager.GetRolesAsync(user);

            if (oldRoles.Any())
            {
                var removeResult =
                    await _userManager.RemoveFromRolesAsync(
                        user,
                        oldRoles);

                if (!removeResult.Succeeded)
                    return removeResult;
            }

            return await _userManager.AddToRoleAsync(
                user,
                model.Role);
        }
    }
}