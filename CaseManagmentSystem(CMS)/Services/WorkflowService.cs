using CaseManagementSystem.Constants;
using CaseManagementSystem.Data;
using CaseManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace CaseManagementSystem.Services
{
    public class WorkflowService : IWorkflowService
    {
        private readonly ApplicationDbContext _db;

        public WorkflowService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<bool> ChangeStageAsync(
            int caseId,
            string newStageName,
            string performedByUserId,
            string? comment = null)
        {
            var caseItem = await _db.Cases
                .Include(x => x.CurrentWorkflowStage)
                .FirstOrDefaultAsync(x => x.Id == caseId);

            var newStage = await _db.WorkflowStages
                .FirstOrDefaultAsync(x => x.Name == newStageName);

            if (caseItem == null || newStage == null)
                return false;

            // Completed is a final stage.
            if (caseItem.CurrentWorkflowStage?.Name
                == WorkflowStageNames.Completed)
            {
                return false;
            }

            // Only transitions defined in WorkflowTransitions are allowed.
            var isAllowed = await _db.WorkflowTransitions
                .AnyAsync(x =>
                    x.FromStageId == caseItem.CurrentWorkflowStageId &&
                    x.ToStageId == newStage.Id);

            if (!isAllowed)
                return false;

            var oldStageId = caseItem.CurrentWorkflowStageId;

            caseItem.CurrentWorkflowStageId = newStage.Id;

            _db.CaseStatusHistories.Add(
                new CaseStatusHistory
                {
                    CaseId = caseId,
                    PerformedByUserId = performedByUserId,
                    Action = $"Stage changed to {newStageName}",
                    PreviousStageId = oldStageId,
                    NewStageId = newStage.Id,
                    Comment = comment,
                    CreatedAt = DateTime.UtcNow
                });

            await _db.SaveChangesAsync();

            return true;
        }

        public async Task<bool> StartChallengeAsync(
            int caseId,
            string userId,
            string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
                return false;

            var caseItem = await _db.Cases
                .Include(x => x.CurrentWorkflowStage)
                .FirstOrDefaultAsync(x => x.Id == caseId);

            if (caseItem == null
                || caseItem.AssignedExpertId != userId
                || caseItem.CurrentWorkflowStage?.Name
                    != WorkflowStageNames.InProgress)
            {
                return false;
            }

            var challengeStage = await _db.WorkflowStages
                .FirstAsync(x =>
                    x.Name == WorkflowStageNames.Challenge);

            var oldStageId =
                caseItem.CurrentWorkflowStageId;

            caseItem.CurrentWorkflowStageId =
                challengeStage.Id;

            _db.CaseChallenges.Add(
                new CaseChallenge
                {
                    CaseId = caseId,
                    Reason = reason.Trim(),
                    StartedByUserId = userId,
                    StartedAt = DateTime.UtcNow
                });

            _db.CaseStatusHistories.Add(
                new CaseStatusHistory
                {
                    CaseId = caseId,
                    PerformedByUserId = userId,
                    Action = "Challenge started",
                    PreviousStageId = oldStageId,
                    NewStageId = challengeStage.Id,
                    Comment = reason.Trim(),
                    CreatedAt = DateTime.UtcNow
                });

            await _db.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ResolveChallengeAsync(
            int caseId,
            string supervisorUserId,
            string comment)
        {
            if (string.IsNullOrWhiteSpace(comment))
                return false;

            var caseItem = await _db.Cases
                .Include(x => x.CurrentWorkflowStage)
                .FirstOrDefaultAsync(x => x.Id == caseId);

            if (caseItem == null
                || caseItem.CurrentWorkflowStage?.Name
                    != WorkflowStageNames.Challenge)
            {
                return false;
            }

            var challenge = await _db.CaseChallenges
                .Where(x =>
                    x.CaseId == caseId &&
                    x.ResolvedAt == null)
                .OrderByDescending(x => x.StartedAt)
                .FirstOrDefaultAsync();

            if (challenge == null)
                return false;

            var inProgressStage =
                await _db.WorkflowStages
                    .FirstAsync(x =>
                        x.Name ==
                        WorkflowStageNames.InProgress);

            var oldStageId =
                caseItem.CurrentWorkflowStageId;

            var now = DateTime.UtcNow;

            challenge.ResolvedByUserId =
                supervisorUserId;

            challenge.ResolvedAt =
                now;

            challenge.SupervisorComment =
                comment.Trim();

            caseItem.CurrentWorkflowStageId =
                inProgressStage.Id;

            _db.CaseStatusHistories.Add(
                new CaseStatusHistory
                {
                    CaseId = caseId,
                    PerformedByUserId = supervisorUserId,
                    Action = "Challenge resolved",
                    PreviousStageId = oldStageId,
                    NewStageId = inProgressStage.Id,
                    Comment = comment.Trim(),
                    CreatedAt = now
                });

            await _db.SaveChangesAsync();

            return true;
        }

        public async Task<bool> SubmitCompletionAsync(
            int caseId,
            string userId,
            string summary)
        {
            if (string.IsNullOrWhiteSpace(summary))
                return false;

            var caseItem = await _db.Cases
                .Include(x => x.CurrentWorkflowStage)
                .FirstOrDefaultAsync(x => x.Id == caseId);

            if (caseItem == null
                || caseItem.AssignedExpertId != userId
                || caseItem.CurrentWorkflowStage?.Name
                    != WorkflowStageNames.InProgress)
            {
                return false;
            }

            var pendingStage =
                await _db.WorkflowStages
                    .FirstAsync(x =>
                        x.Name ==
                        WorkflowStageNames.CompletionPending);

            var oldStageId =
                caseItem.CurrentWorkflowStageId;

            var now = DateTime.UtcNow;

            caseItem.CompletionSummary =
                summary.Trim();

            caseItem.CompletionRequestedAt =
                now;

            caseItem.CurrentWorkflowStageId =
                pendingStage.Id;

            _db.CaseStatusHistories.Add(
                new CaseStatusHistory
                {
                    CaseId = caseId,
                    PerformedByUserId = userId,
                    Action = "Completion submitted",
                    PreviousStageId = oldStageId,
                    NewStageId = pendingStage.Id,
                    Comment = summary.Trim(),
                    CreatedAt = now
                });

            await _db.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ApproveCompletionAsync(
            int caseId,
            string supervisorUserId)
        {
            var caseItem = await _db.Cases
                .Include(x => x.CurrentWorkflowStage)
                .FirstOrDefaultAsync(x => x.Id == caseId);

            if (caseItem == null
                || caseItem.CurrentWorkflowStage?.Name
                    != WorkflowStageNames.CompletionPending)
            {
                return false;
            }

            var completedStage =
                await _db.WorkflowStages
                    .FirstAsync(x =>
                        x.Name ==
                        WorkflowStageNames.Completed);

            var oldStageId =
                caseItem.CurrentWorkflowStageId;

            var now = DateTime.UtcNow;

            caseItem.CurrentWorkflowStageId =
                completedStage.Id;

            caseItem.CompletedAt =
                now;

            _db.CaseStatusHistories.Add(
                new CaseStatusHistory
                {
                    CaseId = caseId,
                    PerformedByUserId = supervisorUserId,
                    Action = "Completion approved",
                    PreviousStageId = oldStageId,
                    NewStageId = completedStage.Id,
                    Comment =
                        "Case completed by Supervisor approval.",
                    CreatedAt = now
                });

            await _db.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ReturnToWorkAsync(
            int caseId,
            string supervisorUserId,
            string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
                return false;

            var caseItem = await _db.Cases
                .Include(x => x.CurrentWorkflowStage)
                .FirstOrDefaultAsync(x => x.Id == caseId);

            if (caseItem == null
                || caseItem.CurrentWorkflowStage?.Name
                    != WorkflowStageNames.CompletionPending)
            {
                return false;
            }

            var inProgressStage =
                await _db.WorkflowStages
                    .FirstAsync(x =>
                        x.Name ==
                        WorkflowStageNames.InProgress);

            var oldStageId =
                caseItem.CurrentWorkflowStageId;

            var now = DateTime.UtcNow;

            caseItem.CurrentWorkflowStageId =
                inProgressStage.Id;

            _db.CaseStatusHistories.Add(
                new CaseStatusHistory
                {
                    CaseId = caseId,
                    PerformedByUserId = supervisorUserId,
                    Action =
                        "Completion returned to work",
                    PreviousStageId = oldStageId,
                    NewStageId = inProgressStage.Id,
                    Comment = reason.Trim(),
                    CreatedAt = now
                });

            await _db.SaveChangesAsync();

            return true;
        }
    }
}