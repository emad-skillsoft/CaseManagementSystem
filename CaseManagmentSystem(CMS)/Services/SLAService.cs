namespace CaseManagementSystem.Services
{
    public class SLAService : ISLAService
    {
        public DateTime GetDueDate(DateTime slaStartDate, int allowedHours)
            => throw new NotImplementedException("Implemented in Session 2.");

        public TimeSpan GetTotalDuration(DateTime slaStartDate, DateTime? completedAt, DateTime? asOf = null)
            => throw new NotImplementedException("Implemented in Session 2.");

        public TimeSpan GetRemainingTime(DateTime dueDate, DateTime? completedAt, DateTime? asOf = null)
            => throw new NotImplementedException("Implemented in Session 2.");

        public bool IsDelayed(DateTime dueDate, DateTime? completedAt, DateTime? asOf = null)
            => throw new NotImplementedException("Implemented in Session 3.");
    }
}
