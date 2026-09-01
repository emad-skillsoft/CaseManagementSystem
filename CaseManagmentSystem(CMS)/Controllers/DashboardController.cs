using CaseManagementSystem.Constants;
using CaseManagementSystem.Data;
using CaseManagementSystem.Services;
using CaseManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CaseManagementSystem.Controllers
{
    [Authorize(Roles = RoleNames.Supervisor)]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ISLAService _slaService;

        public DashboardController(
            ApplicationDbContext db,
            ISLAService slaService)
        {
            _db = db;
            _slaService = slaService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string filter = "All")
        {
            var cases = await _db.Cases
                .AsNoTracking()
                .Include(x => x.CurrentWorkflowStage)
                .Include(x => x.AssignedExpert)
                .OrderByDescending(x => x.ImportedAt)
                .ToListAsync();

            var items = new List<DashboardCaseItemViewModel>();

            foreach (var caseItem in cases)
            {
                var sla = await _slaService.GetStatusAsync(caseItem);

                items.Add(new DashboardCaseItemViewModel
                {
                    Id = caseItem.Id,
                    ExternalCaseId = caseItem.ExternalCaseId,
                    Title = caseItem.Title,
                    Priority = caseItem.Priority.ToString(),
                    CurrentStage = caseItem.CurrentWorkflowStage?.Name ?? string.Empty,
                    AssignedExpert = caseItem.AssignedExpert?.FullName ?? string.Empty,
                    DueDate = sla.DueDate,
                    IsDelayed = sla.IsDelayed
                });
            }

            var model = new DashboardViewModel
            {
                TotalCases = items.Count,
                InProgressCount = items.Count(x =>
                    x.CurrentStage == WorkflowStageNames.InProgress),
                ChallengeCount = items.Count(x =>
                    x.CurrentStage == WorkflowStageNames.Challenge),
                DelayedCount = items.Count(x => x.IsDelayed),
                CompletedCount = items.Count(x =>
                    x.CurrentStage == WorkflowStageNames.Completed),
                SelectedFilter = filter
            };

            model.Cases = filter switch
            {
                "In Progress" => items
                    .Where(x => x.CurrentStage == WorkflowStageNames.InProgress)
                    .ToList(),

                "Challenged" => items
                    .Where(x => x.CurrentStage == WorkflowStageNames.Challenge)
                    .ToList(),

                "Delayed" => items
                    .Where(x => x.IsDelayed)
                    .ToList(),

                "Completed" => items
                    .Where(x => x.CurrentStage == WorkflowStageNames.Completed)
                    .ToList(),

                _ => items
            };

            return View(model);
        }
    }
}
