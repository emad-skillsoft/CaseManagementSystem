using CaseManagementSystem.Data;
using CaseManagementSystem.ViewModels;
using CaseManagementSystem.Constants;
using CaseManagementSystem.Models;

using Microsoft.EntityFrameworkCore;

namespace CaseManagementSystem.Services
{
    public class CaseService : ICaseService
    {
        private readonly ApplicationDbContext _db;

        public CaseService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<CaseListViewModel>> GetAllCasesAsync()
        {
            return await _db.Cases.AsNoTracking()
                .Include(x => x.AssignedExpert)
                .Include(x => x.CurrentWorkflowStage)
                .OrderByDescending(x => x.ImportedAt)
                .Select(x => new CaseListViewModel
                {
                    Id = x.Id,
                    ExternalCaseId = x.ExternalCaseId,
                    Title = x.Title,
                    Priority = x.Priority,
                    CurrentStage = x.CurrentWorkflowStage!.Name,
                    AssignedExpertName = x.AssignedExpert!.FullName,
                    SLAStartDate = x.SLAStartDate
                })
                .ToListAsync();
        }

        public async Task<List<CaseListViewModel>> GetMyCasesAsync(string userId)
        {
            return await _db.Cases.AsNoTracking()
                .Include(x => x.AssignedExpert)
                .Include(x => x.CurrentWorkflowStage)
                .Where(x => x.AssignedExpertId == userId)
                .OrderByDescending(x => x.ImportedAt)
                .Select(x => new CaseListViewModel
                {
                    Id = x.Id,
                    ExternalCaseId = x.ExternalCaseId,
                    Title = x.Title,
                    Priority = x.Priority,
                    CurrentStage = x.CurrentWorkflowStage!.Name,
                    AssignedExpertName = x.AssignedExpert!.FullName,
                    SLAStartDate = x.SLAStartDate
                })
                .ToListAsync();
        }

        public async Task<CaseDetailsViewModel?> GetCaseDetailsAsync(int caseId)
        {

            var item = await _db.Cases.AsNoTracking()
                .Include(x => x.AssignedExpert)
                .Include(x => x.CurrentWorkflowStage)
                .Include(x => x.History).ThenInclude(x => x.PreviousStage)
                .Include(x => x.History).ThenInclude(x => x.NewStage)
                .Include(x => x.Updates)
                .ThenInclude(x => x.User)

                .FirstOrDefaultAsync(x => x.Id == caseId);


            if (item == null)
                return null;

            return new CaseDetailsViewModel
            {
                Updates = item.Updates
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new CaseUpdateItemViewModel
                {
                  UserName = u.User?.FullName
                  ?? u.User?.UserName
                  ?? string.Empty,
                  UpdateText = u.UpdateText,
                  CreatedAt = u.CreatedAt
                })
                  .ToList(),


                Id = item.Id,
                ExternalCaseId = item.ExternalCaseId,
                Title = item.Title,
                Description = item.Description,
                Priority = item.Priority,
                SLAStartDate = item.SLAStartDate,
                ImportedAt = item.ImportedAt,
                CompletedAt = item.CompletedAt,
                AssignedExpertId = item.AssignedExpertId,
                AssignedExpertName = item.AssignedExpert?.FullName ?? string.Empty,
                CurrentStage = item.CurrentWorkflowStage?.Name ?? string.Empty,
                History = item.History
                    .OrderByDescending(h => h.CreatedAt)
                    .Select(h => new CaseHistoryItemViewModel
                    {
                        Action = h.Action,
                        PreviousStage = h.PreviousStage?.Name,
                        NewStage = h.NewStage?.Name,
                        Comment = h.Comment,
                        CreatedAt = h.CreatedAt
                    })
                    .ToList()
            };

        }

        public async Task<bool> AddUpdateAsync(
    int caseId,
    string userId,
    string updateText)
        {
            if (string.IsNullOrWhiteSpace(updateText))
                return false;

            var caseItem = await _db.Cases
                .Include(x => x.CurrentWorkflowStage)
                .FirstOrDefaultAsync(x => x.Id == caseId);

            if (caseItem == null || caseItem.AssignedExpertId != userId)
                return false;

            if (caseItem.CurrentWorkflowStage?.Name is not
                (WorkflowStageNames.Assigned or WorkflowStageNames.InProgress))
                return false;

            _db.CaseUpdates.Add(new CaseUpdate
            {
                CaseId = caseId,
                UserId = userId,
                UpdateText = updateText.Trim(),
                CreatedAt = DateTime.UtcNow
            });

            var oldStageId = caseItem.CurrentWorkflowStageId;
            var oldStageName = caseItem.CurrentWorkflowStage!.Name;

            if (oldStageName == WorkflowStageNames.Assigned)
            {
                var inProgress = await _db.WorkflowStages
                    .FirstAsync(x => x.Name == WorkflowStageNames.InProgress);

                caseItem.CurrentWorkflowStageId = inProgress.Id;

                _db.CaseStatusHistories.Add(new CaseStatusHistory
                {
                    CaseId = caseId,
                    PerformedByUserId = userId,
                    Action = "Work started / update added",
                    PreviousStageId = oldStageId,
                    NewStageId = inProgress.Id,
                    Comment = updateText.Trim(),
                    CreatedAt = DateTime.UtcNow
                });
            }
            else
            {
                _db.CaseStatusHistories.Add(new CaseStatusHistory
                {
                    CaseId = caseId,
                    PerformedByUserId = userId,
                    Action = "Update added",
                    PreviousStageId = oldStageId,
                    NewStageId = oldStageId,
                    Comment = updateText.Trim(),
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _db.SaveChangesAsync();
            return true;
        }

    }
}
