using CaseManagementSystem.Data;
using CaseManagementSystem.ViewModels;

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
                .FirstOrDefaultAsync(x => x.Id == caseId);

            if (item == null)
                return null;

            return new CaseDetailsViewModel
            {
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

        public Task<bool> AddUpdateAsync(int caseId, string userId, string updateText)
        {
            // Implemented in Session 3.
            return Task.FromResult(false);
        }
    }
}
