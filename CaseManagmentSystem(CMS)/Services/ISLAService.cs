namespace CaseManagementSystem.Services
{
    public interface ISLAService
    {
        DateTime GetDueDate(DateTime slaStartDate, int allowedHours);
        TimeSpan GetTotalDuration(DateTime slaStartDate, DateTime? completedAt, DateTime? asOf = null);
        TimeSpan GetRemainingTime(DateTime dueDate, DateTime? completedAt, DateTime? asOf = null);
        bool IsDelayed(DateTime dueDate, DateTime? completedAt, DateTime? asOf = null);
    }
}
