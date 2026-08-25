using CaseManagementSystem.Enums;

namespace CaseManagementSystem.Services
{
    public interface ISLAService
    {
        Task<DateTime?> GetDueDateAsync(CasePriority priority, DateTime slaStartDate);
        TimeSpan GetTotalDuration(DateTime slaStartDate, DateTime? completedAt, DateTime? asOf = null);
        TimeSpan GetRemainingTime(DateTime dueDate, DateTime? completedAt, DateTime? asOf = null);
        bool IsDelayed(DateTime dueDate, DateTime? completedAt, DateTime? asOf = null);
    }
}
