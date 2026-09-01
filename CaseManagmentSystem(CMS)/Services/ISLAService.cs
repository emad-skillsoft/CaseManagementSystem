using CaseManagementSystem.Enums;
using CaseManagementSystem.Models;
using CaseManagementSystem.ViewModels;


namespace CaseManagementSystem.Services
{
    public interface ISLAService
    {
        Task<DateTime?> GetDueDateAsync(CasePriority priority, DateTime slaStartDate);
        TimeSpan GetTotalDuration(DateTime slaStartDate, DateTime? completedAt, DateTime? asOf = null);
        TimeSpan GetRemainingTime(DateTime dueDate, DateTime? completedAt, DateTime? asOf = null);
        bool IsDelayed(DateTime dueDate, DateTime? completedAt, DateTime? asOf = null);
        Task<TimeSpan> GetInProgressDurationAsync(int caseId, DateTime? asOf = null);
        Task<SLAStatusViewModel> GetStatusAsync(Case caseItem, DateTime? asOf = null);
        Task<TimeSpan> GetChallengeDurationAsync(int caseId, DateTime? asOf = null);

    }
}
