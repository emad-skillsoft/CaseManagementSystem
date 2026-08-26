
using CaseManagementSystem.Data;
using CaseManagementSystem.Enums;
using CaseManagementSystem.Models;
using WorkflowStages = CaseManagementSystem.Constants.WorkflowStageNames;
using Microsoft.EntityFrameworkCore;
using CaseManagementSystem.Constants;
using CaseManagementSystem.Models;
using CaseManagementSystem.ViewModels;


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
        public async Task<TimeSpan> GetInProgressDurationAsync(
    int caseId,
    DateTime? asOf = null)
        {
            var history = await _db.CaseStatusHistories.AsNoTracking()
                .Where(x => x.CaseId == caseId && x.PreviousStageId != x.NewStageId)
                .Include(x => x.PreviousStage)
                .Include(x => x.NewStage)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync();

            var total = TimeSpan.Zero;
            DateTime? started = null;

            foreach (var historyItem in history)
            {
                var enteredInProgress =
                    historyItem.PreviousStage?.Name != WorkflowStages.InProgress
                    && historyItem.NewStage?.Name == WorkflowStages.InProgress;

                var leftInProgress =
                    historyItem.PreviousStage?.Name == WorkflowStages.InProgress
                    && historyItem.NewStage?.Name != WorkflowStages.InProgress;

                if (enteredInProgress)
                    started = historyItem.CreatedAt;

                if (leftInProgress && started.HasValue)
                {
                    total += historyItem.CreatedAt - started.Value;
                    started = null;
                }
            }

            if (started.HasValue)
                total += (asOf ?? DateTime.UtcNow) - started.Value;

            return total < TimeSpan.Zero
                ? TimeSpan.Zero
                : total;
        }
        public async Task<SLAStatusViewModel> GetStatusAsync(Case caseItem, DateTime? asOf = null)
        {
            // Implementation for getting SLA status
            throw new NotImplementedException();
        }
    }
}
