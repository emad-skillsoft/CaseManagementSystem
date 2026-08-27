using global::CaseManagementSystem.Data;
using global::CaseManagementSystem.Models;
using global::CaseManagementSystem.Services;
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
            var caseItem = await _db.Cases.FindAsync(caseId);
            var newStage = await _db.WorkflowStages
                .FirstOrDefaultAsync(x => x.Name == newStageName);

            if (caseItem == null || newStage == null)
                return false;

            var oldStageId = caseItem.CurrentWorkflowStageId;
            caseItem.CurrentWorkflowStageId = newStage.Id;

            _db.CaseStatusHistories.Add(new CaseStatusHistory
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
                || caseItem.CurrentWorkflowStage?.Name != WorkflowStageNames.InProgress)
                return false;

            var challengeStage = await _db.WorkflowStages
                .FirstAsync(x => x.Name == WorkflowStageNames.Challenge);

            var oldStageId = caseItem.CurrentWorkflowStageId;
            caseItem.CurrentWorkflowStageId = challengeStage.Id;

            _db.CaseChallenges.Add(new CaseChallenge
            {
                CaseId = caseId,
                Reason = reason.Trim(),
                StartedByUserId = userId,
                StartedAt = DateTime.UtcNow
            });

            _db.CaseStatusHistories.Add(new CaseStatusHistory
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
                || caseItem.CurrentWorkflowStage?.Name != WorkflowStageNames.Challenge)
                return false;

            var challenge = await _db.CaseChallenges
                .Where(x => x.CaseId == caseId && x.ResolvedAt == null)
                .OrderByDescending(x => x.StartedAt)
                .FirstOrDefaultAsync();

            if (challenge == null)
                return false;

            var inProgress = await _db.WorkflowStages
                .FirstAsync(x => x.Name == WorkflowStageNames.InProgress);

            var oldStageId = caseItem.CurrentWorkflowStageId;

            challenge.ResolvedByUserId = supervisorUserId;
            challenge.ResolvedAt = DateTime.UtcNow;
            challenge.SupervisorComment = comment.Trim();

            caseItem.CurrentWorkflowStageId = inProgress.Id;

            _db.CaseStatusHistories.Add(new CaseStatusHistory
            {
                CaseId = caseId,
                PerformedByUserId = supervisorUserId,
                Action = "Challenge resolved",
                PreviousStageId = oldStageId,
                NewStageId = inProgress.Id,
                Comment = comment.Trim(),
                CreatedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
            return true;
        }

        public Task<bool> SubmitCompletionAsync(int caseId, string userId, string summary)
        {
            // Session 5.
            return Task.FromResult(false);
        }

        public Task<bool> ApproveCompletionAsync(int caseId, string supervisorUserId)
        {
            // Session 5.
            return Task.FromResult(false);
        }

        public Task<bool> ReturnToWorkAsync(int caseId, string supervisorUserId, string reason)
        {
            // Session 5.
            return Task.FromResult(false);
        }
    }
}

