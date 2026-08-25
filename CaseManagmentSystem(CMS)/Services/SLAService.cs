using CaseManagementSystem.Data;
using CaseManagementSystem.Enums;
using Microsoft.EntityFrameworkCore;

namespace CaseManagementSystem.Services
{
    public class SLAService : ISLAService
    {
        private readonly ApplicationDbContext _db;

        public SLAService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<DateTime?> GetDueDateAsync(CasePriority priority, DateTime slaStartDate)
        {
            var config = await _db.SLAConfigurations.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Priority == priority);

            return config == null
                ? null
                : slaStartDate.AddHours(config.AllowedHours);
        }

        public TimeSpan GetTotalDuration(
            DateTime slaStartDate,
            DateTime? completedAt,
            DateTime? asOf = null)
        {
            var end = completedAt ?? asOf ?? DateTime.UtcNow;
            return end <= slaStartDate
                ? TimeSpan.Zero
                : end - slaStartDate;
        }

        public TimeSpan GetRemainingTime(
            DateTime dueDate,
            DateTime? completedAt,
            DateTime? asOf = null)
        {
            var end = completedAt ?? asOf ?? DateTime.UtcNow;
            return dueDate - end;
        }

        public bool IsDelayed(
            DateTime dueDate,
            DateTime? completedAt,
            DateTime? asOf = null)
        {
            var end = completedAt ?? asOf ?? DateTime.UtcNow;
            return end > dueDate;
        }
    }
}
