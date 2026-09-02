using CaseManagementSystem.Data;
using CaseManagementSystem.Enums;
using CaseManagementSystem.Models;
using CaseManagementSystem.ViewModels;
using Microsoft.EntityFrameworkCore;

using WorkflowStages =
    CaseManagementSystem.Constants.WorkflowStageNames;

namespace CaseManagementSystem.Services
{
    public class SLAService : ISLAService
    {
        private readonly ApplicationDbContext _db;

        public SLAService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<DateTime?> GetDueDateAsync(
            CasePriority priority,
            DateTime slaStartDate)
        {
            var config = await _db.SLAConfigurations
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Priority == priority);

            return config == null
                ? null
                : slaStartDate.AddHours(config.AllowedHours);
        }

        public TimeSpan GetTotalDuration(
            DateTime slaStartDate,
            DateTime? completedAt,
            DateTime? asOf = null)
        {
            var end =
                completedAt
                ?? asOf
                ?? DateTime.UtcNow;

            return end <= slaStartDate
                ? TimeSpan.Zero
                : end - slaStartDate;
        }

        public TimeSpan GetRemainingTime(
            DateTime dueDate,
            DateTime? completedAt,
            DateTime? asOf = null)
        {
            var end =
                completedAt
                ?? asOf
                ?? DateTime.UtcNow;

            return dueDate - end;
        }

        public bool IsDelayed(
            DateTime dueDate,
            DateTime? completedAt,
            DateTime? asOf = null)
        {
            var end =
                completedAt
                ?? asOf
                ?? DateTime.UtcNow;

            return end > dueDate;
        }

        public async Task<TimeSpan>
            GetInProgressDurationAsync(
                int caseId,
                DateTime? asOf = null)
        {
            var history = await _db.CaseStatusHistories
                .AsNoTracking()
                .Where(x => x.CaseId == caseId)
                .Include(x => x.PreviousStage)
                .Include(x => x.NewStage)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync();

            var total = TimeSpan.Zero;
            DateTime? started = null;

            foreach (var historyItem in history)
            {
                var enteredInProgress =
                    historyItem.PreviousStage?.Name
                        != WorkflowStages.InProgress
                    &&
                    historyItem.NewStage?.Name
                        == WorkflowStages.InProgress;

                var leftInProgress =
                    historyItem.PreviousStage?.Name
                        == WorkflowStages.InProgress
                    &&
                    historyItem.NewStage?.Name
                        != WorkflowStages.InProgress;

                if (enteredInProgress)
                {
                    started = historyItem.CreatedAt;
                }

                if (leftInProgress &&
                    started.HasValue)
                {
                    total +=
                        historyItem.CreatedAt
                        - started.Value;

                    started = null;
                }
            }

            if (started.HasValue)
            {
                var end =
                    asOf ?? DateTime.UtcNow;

                if (end > started.Value)
                {
                    total +=
                        end - started.Value;
                }
            }

            return total < TimeSpan.Zero
                ? TimeSpan.Zero
                : total;
        }

        public async Task<TimeSpan>
            GetChallengeDurationAsync(
                int caseId,
                DateTime? asOf = null)
        {
            var challenges = await _db.CaseChallenges
                .AsNoTracking()
                .Where(x => x.CaseId == caseId)
                .ToListAsync();

            var now =
                asOf ?? DateTime.UtcNow;

            var total =
                TimeSpan.Zero;

            foreach (var challenge in challenges)
            {
                var end =
                    challenge.ResolvedAt
                    ?? now;

                if (end > challenge.StartedAt)
                {
                    total +=
                        end - challenge.StartedAt;
                }
            }

            return total;
        }

        public async Task<SLAStatusViewModel>
            GetStatusAsync(
                Case caseItem,
                DateTime? asOf = null)
        {
            var dueDate =
                await GetDueDateAsync(
                    caseItem.Priority,
                    caseItem.SLAStartDate);

            var totalDuration =
                GetTotalDuration(
                    caseItem.SLAStartDate,
                    caseItem.CompletedAt,
                    asOf);

            var inProgressDuration =
                await GetInProgressDurationAsync(
                    caseItem.Id,
                    asOf);

            var challengeDuration =
                await GetChallengeDurationAsync(
                    caseItem.Id,
                    asOf);

            return new SLAStatusViewModel
            {
                SLAStartDate =
                    caseItem.SLAStartDate,

                DueDate =
                    dueDate,

                TotalDuration =
                    totalDuration,

                InProgressDuration =
                    inProgressDuration,

                ChallengeDuration =
                    challengeDuration,

                RemainingTime =
                    dueDate.HasValue
                        ? GetRemainingTime(
                            dueDate.Value,
                            caseItem.CompletedAt,
                            asOf)
                        : TimeSpan.Zero,

                IsDelayed =
                    dueDate.HasValue
                    && IsDelayed(
                        dueDate.Value,
                        caseItem.CompletedAt,
                        asOf)
            };
        }
    }
}