using CaseManagementSystem.Constants;
using CaseManagementSystem.Models;
using CaseManagementSystem.ViewModels;
using CaseManagementSystem.Services;
using CaseManagmentSystem_CMS_.ViewModels;
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

        public Task<EditUserViewModel?> GetUserForEditAsync(
            string id)
        {
            // Implemented in Session 4.
            return Task.FromResult<EditUserViewModel?>(null);
        }

        public Task<IdentityResult> UpdateUserAsync(
            EditUserViewModel model)
        {
            // Implemented in Session 4.
            return Task.FromResult(
                IdentityResult.Failed(
                    new IdentityError
                    {
                        Description =
                            "Edit is implemented in Session 4."
                    }));
        }
    }
}