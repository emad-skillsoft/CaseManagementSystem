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
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;
        private readonly ISLAService _slaService;

        public HomeController(
            ILogger<HomeController> logger,
            ApplicationDbContext db,
            ISLAService slaService)
        {
            _logger = logger;
            _db = db;
            _slaService = slaService;
        }


        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var isSupervisor =
                User.IsInRole(RoleNames.Supervisor);

            var isExpert =
                User.IsInRole(RoleNames.Expert);


            var userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier
                );


            /*
             * -------------------------------------------------
             * BASE CASE QUERY
             * -------------------------------------------------
             *
             * Supervisor:
             *      sees all Cases.
             *
             * Expert:
             *      sees only Cases assigned to that Expert.
             *
             */

            var query =
                _db.Cases
                    .AsNoTracking()
                    .Include(x => x.CurrentWorkflowStage)
                    .Include(x => x.AssignedExpert)
                    .AsQueryable();


            if (isExpert && !isSupervisor)
            {
                query =
                    query.Where(
                        x => x.AssignedExpertId == userId
                    );
            }


            var cases =
                await query
                    .OrderByDescending(x => x.ImportedAt)
                    .ToListAsync();


            /*
             * -------------------------------------------------
             * BUILD HOME CASE ITEMS
             * -------------------------------------------------
             */

            var items =
                new List<DashboardCaseItemViewModel>();


            foreach (var caseItem in cases)
            {
                var sla =
                    await _slaService
                        .GetStatusAsync(caseItem);


                items.Add(
                    new DashboardCaseItemViewModel
                    {
                        Id =
                            caseItem.Id,

                        ExternalCaseId =
                            caseItem.ExternalCaseId,

                        Title =
                            caseItem.Title,

                        Priority =
                            caseItem.Priority.ToString(),

                        CurrentStage =
                            caseItem.CurrentWorkflowStage?.Name
                            ?? string.Empty,

                        AssignedExpert =
                            caseItem.AssignedExpert?.FullName
                            ?? string.Empty,

                        DueDate =
                            sla.DueDate,

                        IsDelayed =
                            sla.IsDelayed
                    }
                );
            }


            /*
             * -------------------------------------------------
             * HOME VIEW MODEL
             * -------------------------------------------------
             */

            var model =
                new DashboardViewModel
                {
                    TotalCases =
                        items.Count,

                    InProgressCount =
                        items.Count(
                            x =>
                                x.CurrentStage ==
                                WorkflowStageNames.InProgress
                        ),

                    ChallengeCount =
                        items.Count(
                            x =>
                                x.CurrentStage ==
                                WorkflowStageNames.Challenge
                        ),

                    DelayedCount =
                        items.Count(
                            x => x.IsDelayed
                        ),

                    CompletedCount =
                        items.Count(
                            x =>
                                x.CurrentStage ==
                                WorkflowStageNames.Completed
                        ),

                    SelectedFilter =
                        "All",

                    Cases =
                        items
                };


            /*
             * -------------------------------------------------
             * SUPERVISOR IMPORT NEEDS REVIEW
             * -------------------------------------------------
             */

            if (isSupervisor)
            {
                ViewBag.NeedsReviewCount =
                    await _db.ImportReviewItems
                        .AsNoTracking()
                        .CountAsync(
                            x => !x.IsResolved
                        );
            }
            else
            {
                ViewBag.NeedsReviewCount = 0;
            }


            return View(model);
        }


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
                }
            );
        }
    }
}