using CaseManagementSystem.Constants;
using CaseManagementSystem.Data;
using CaseManagementSystem.Models;
using CaseManagementSystem.Services;
using CaseManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;

namespace CaseManagementSystem.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ISLAService _slaService;

        public HomeController(
            ApplicationDbContext db,
            ISLAService slaService)
        {
            _db = db;
            _slaService = slaService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!User.IsInRole(RoleNames.Supervisor) &&
                !User.IsInRole(RoleNames.Expert))
            {
                return Forbid();
            }

            var query = _db.Cases
                .AsNoTracking()
                .Include(x => x.CurrentWorkflowStage)
                .Include(x => x.AssignedExpert)
                .AsQueryable();

            // Expert sees only his own assigned Cases
            if (User.IsInRole(RoleNames.Expert) &&
                !User.IsInRole(RoleNames.Supervisor))
            {
                var userId = User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

                if (string.IsNullOrWhiteSpace(userId))
                    return Forbid();

                query = query.Where(x =>
                    x.AssignedExpertId == userId);
            }

            var cases = await query
                .OrderByDescending(x => x.ImportedAt)
                .ToListAsync();

            var items = new List<DashboardCaseItemViewModel>();

            foreach (var caseItem in cases)
            {
                var sla = await _slaService
                    .GetStatusAsync(caseItem);

                items.Add(new DashboardCaseItemViewModel
                {
                    Id = caseItem.Id,
                    ExternalCaseId = caseItem.ExternalCaseId,
                    Title = caseItem.Title,
                    Priority = caseItem.Priority.ToString(),
                    CurrentStage =
                        caseItem.CurrentWorkflowStage?.Name
                        ?? string.Empty,
                    AssignedExpert =
                        caseItem.AssignedExpert?.FullName
                        ?? string.Empty,
                    DueDate = sla.DueDate,
                    IsDelayed = sla.IsDelayed
                });
            }

            var model = new DashboardViewModel
            {
                TotalCases = items.Count,

                InProgressCount = items.Count(x =>
                    x.CurrentStage ==
                    WorkflowStageNames.InProgress),

                ChallengeCount = items.Count(x =>
                    x.CurrentStage ==
                    WorkflowStageNames.Challenge),

                DelayedCount = items.Count(x =>
                    x.IsDelayed),

                CompletedCount = items.Count(x =>
                    x.CurrentStage ==
                    WorkflowStageNames.Completed),

                SelectedFilter = "All",

                Cases = items
            };

            return View(model);
        }

        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Error()
        {
            return View(
                new ErrorViewModel
                {
                    RequestId =
                        Activity.Current?.Id
                        ?? HttpContext.TraceIdentifier
                });
        }
    }
}