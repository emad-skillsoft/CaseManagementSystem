using CaseManagementSystem.Models;
using CaseManagementSystem.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace CaseManagementSystem.Services
{
    public interface IUserService
    {
        Task<List<ApplicationUser>> GetUsersAsync();

        Task<ApplicationUser?> GetExpertByEmployeeNumberAsync(
            string employeeNumber);

        Task<IdentityResult> CreateUserAsync(
            CreateUserViewModel model);

        Task<EditUserViewModel?> GetUserForEditAsync(
            string id);

        Task<IdentityResult> UpdateUserAsync(
            EditUserViewModel model);
    }
}