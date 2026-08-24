using CaseManagementSystem.Models;
using CaseManagmentSystem_CMS_.Models;
using CaseManagmentSystem_CMS_.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace CaseManagmentSystem_CMS_.Services
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