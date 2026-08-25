using CaseManagementSystem.ViewModels;

namespace CaseManagementSystem.Services
{
    public interface ICaseService
    {
        Task<List<CaseListViewModel>> GetAllCasesAsync();

        Task<List<CaseListViewModel>> GetMyCasesAsync(string userId);

        Task<CaseDetailsViewModel?> GetCaseDetailsAsync(int caseId);
    }
}