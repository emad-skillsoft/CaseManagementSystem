using CaseManagementSystem.Models;

namespace CaseManagementSystem.Services
{
    public interface ICaseService
    {
        Task<List<Case>> GetAllCasesAsync();
        Task<List<Case>> GetMyCasesAsync(string userId);
        Task<Case?> GetCaseDetailsAsync(int caseId);
        Task<bool> AddUpdateAsync(int caseId, string userId, string updateText);
    }
}
