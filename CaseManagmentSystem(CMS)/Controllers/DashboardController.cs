using CaseManagementSystem.Constants;
using CaseManagementSystem.Data;
using CasePriority = CaseManagementSystem.Enums.CasePriority;
using CaseManagementSystem.Models;
using CaseManagementSystem.Services;
using CaseManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CaseManagementSystem.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ISLAService _slaService;
        private readonly UserManager<ApplicationUser> _userManager;


        public DashboardController(
            ApplicationDbContext db,
            ISLAService slaService,
            UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _slaService = slaService;
            _userManager = userManager;
        }


        [HttpGet]
        public async Task<IActionResult> Index(
            DateTime? fromDate = null,
            DateTime? toDate = null,
            string expertId = "",
            string stage = "All",
            string priority = "All",
            string slaStatus = "All",
            int page = 1,
            int pageSize = 10)
        {
            // =================================================
            // ROLE
            // =================================================

            var isSupervisor =
                User.IsInRole(RoleNames.Supervisor);

            var isExpert =
                User.IsInRole(RoleNames.Expert);


            if (!isSupervisor && !isExpert)
            {
                return Forbid();
            }


            var currentUserId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier
                ) ?? string.Empty;


            // =================================================
            // EXPERT LIST
            // =================================================

            var expertUsers =
                (await _userManager
                    .GetUsersInRoleAsync(RoleNames.Expert))
                    .OrderBy(x => x.FullName)
                    .ToList();


            /*
             * Supervisor can choose any Expert.
             *
             * Expert is ALWAYS forced to their own account.
             * A user cannot manipulate the query string to see
             * another Expert's Dashboard data.
             */

            var effectiveExpertId =
                isSupervisor
                    ? expertId?.Trim() ?? string.Empty
                    : currentUserId;


            // =================================================
            // NORMALIZE PAGINATION
            // =================================================

            page =
                Math.Max(
                    1,
                    page
                );


            var allowedPageSizes =
                new[]
                {
                    5,
                    10,
                    20,
                    50
                };


            if (!allowedPageSizes.Contains(pageSize))
            {
                pageSize = 10;
            }


            // =================================================
            // BASE QUERY
            // =================================================

            var query =
                _db.Cases
                    .AsNoTracking()
                    .Include(x => x.CurrentWorkflowStage)
                    .Include(x => x.AssignedExpert)
                    .AsQueryable();


            // =================================================
            // EXPERT FILTER
            // =================================================

            if (!string.IsNullOrWhiteSpace(effectiveExpertId))
            {
                query =
                    query.Where(
                        x =>
                            x.AssignedExpertId ==
                            effectiveExpertId
                    );
            }


            // =================================================
            // DATE RANGE FILTER
            //
            // Date Range is based on SLAStartDate.
            // =================================================

            if (fromDate.HasValue)
            {
                var from =
                    fromDate.Value.Date;


                query =
                    query.Where(
                        x =>
                            x.SLAStartDate >= from
                    );
            }


            if (toDate.HasValue)
            {
                /*
                 * Use next-day exclusive comparison so the whole
                 * selected To Date is included.
                 */

                var exclusiveTo =
                    toDate.Value.Date.AddDays(1);


                query =
                    query.Where(
                        x =>
                            x.SLAStartDate < exclusiveTo
                    );
            }


            // =================================================
            // STAGE FILTER
            // =================================================

            var validStages =
                new[]
                {
                    WorkflowStageNames.Assigned,
                    WorkflowStageNames.InProgress,
                    WorkflowStageNames.Challenge,
                    WorkflowStageNames.CompletionPending,
                    WorkflowStageNames.Completed
                };


            if (
                stage != "All" &&
                validStages.Contains(stage)
            )
            {
                query =
                    query.Where(
                        x =>
                            x.CurrentWorkflowStage != null &&
                            x.CurrentWorkflowStage.Name == stage
                    );
            }
            else
            {
                stage = "All";
            }


            // =================================================
            // PRIORITY FILTER
            // =================================================

            if (
                priority != "All" &&
                Enum.TryParse<CasePriority>(
                    priority,
                    true,
                    out var parsedPriority
                )
            )
            {
                query =
                    query.Where(
                        x =>
                            x.Priority ==
                            parsedPriority
                    );
            }
            else
            {
                priority = "All";
            }


            // =================================================
            // LOAD FILTERED CASES
            // =================================================

            var caseEntities =
                await query
                    .OrderByDescending(x => x.ImportedAt)
                    .ToListAsync();


            // =================================================
            // SLA CALCULATION
            // =================================================

            var items =
                new List<DashboardCaseItemViewModel>();


            foreach (var caseItem in caseEntities)
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

                        AssignedExpertId =
                            caseItem.AssignedExpertId,

                        AssignedExpert =
                            caseItem.AssignedExpert?.FullName
                            ?? string.Empty,

                        SLAStartDate =
                            caseItem.SLAStartDate,

                        DueDate =
                            sla.DueDate,

                        IsDelayed =
                            sla.IsDelayed
                    }
                );
            }


            // =================================================
            // SLA FILTER
            //
            // Delayed is NOT a workflow stage.
            // =================================================

            if (slaStatus == "Delayed")
            {
                items =
                    items
                        .Where(x => x.IsDelayed)
                        .ToList();
            }
            else if (slaStatus == "OnTrack")
            {
                items =
                    items
                        .Where(x => !x.IsDelayed)
                        .ToList();
            }
            else
            {
                slaStatus = "All";
            }


            // =================================================
            // COUNTS
            // =================================================

            var totalCases =
                items.Count;


            var assignedCount =
                items.Count(
                    x =>
                        x.CurrentStage ==
                        WorkflowStageNames.Assigned
                );


            var inProgressCount =
                items.Count(
                    x =>
                        x.CurrentStage ==
                        WorkflowStageNames.InProgress
                );


            var challengeCount =
                items.Count(
                    x =>
                        x.CurrentStage ==
                        WorkflowStageNames.Challenge
                );


            var completionPendingCount =
                items.Count(
                    x =>
                        x.CurrentStage ==
                        WorkflowStageNames.CompletionPending
                );


            var completedCount =
                items.Count(
                    x =>
                        x.CurrentStage ==
                        WorkflowStageNames.Completed
                );


            var delayedCount =
                items.Count(
                    x => x.IsDelayed
                );


            var onTrackCount =
                items.Count(
                    x => !x.IsDelayed
                );


            var compliancePercentage =
                totalCases == 0
                    ? 0
                    : Math.Round(
                        onTrackCount * 100.0 /
                        totalCases,
                        1
                    );


            // =================================================
            // EXPERT WORKLOAD
            // =================================================

            var workload =
                new List<DashboardExpertWorkloadViewModel>();


            if (isSupervisor)
            {
                var workloadExperts =
                    string.IsNullOrWhiteSpace(
                        effectiveExpertId
                    )
                        ? expertUsers
                        : expertUsers
                            .Where(
                                x =>
                                    x.Id ==
                                    effectiveExpertId
                            )
                            .ToList();


                foreach (var expert in workloadExperts)
                {
                    var expertItems =
                        items
                            .Where(
                                x =>
                                    x.AssignedExpertId ==
                                    expert.Id
                            )
                            .ToList();


                    workload.Add(
                        new DashboardExpertWorkloadViewModel
                        {
                            ExpertId =
                                expert.Id,

                            ExpertName =
                                expert.FullName,

                            EmployeeNumber =
                                expert.EmployeeNumber,

                            ActiveCount =
                                expertItems.Count(
                                    x =>
                                        x.CurrentStage !=
                                        WorkflowStageNames.Completed
                                ),

                            AssignedCount =
                                expertItems.Count(
                                    x =>
                                        x.CurrentStage ==
                                        WorkflowStageNames.Assigned
                                ),

                            InProgressCount =
                                expertItems.Count(
                                    x =>
                                        x.CurrentStage ==
                                        WorkflowStageNames.InProgress
                                ),

                            ChallengeCount =
                                expertItems.Count(
                                    x =>
                                        x.CurrentStage ==
                                        WorkflowStageNames.Challenge
                                ),

                            CompletionPendingCount =
                                expertItems.Count(
                                    x =>
                                        x.CurrentStage ==
                                        WorkflowStageNames.CompletionPending
                                ),

                            CompletedCount =
                                expertItems.Count(
                                    x =>
                                        x.CurrentStage ==
                                        WorkflowStageNames.Completed
                                ),

                            DelayedCount =
                                expertItems.Count(
                                    x =>
                                        x.IsDelayed
                                )
                        }
                    );
                }
            }


            // =================================================
            // PAGINATION
            // =================================================

            var pageCount =
                Math.Max(
                    1,
                    (int)Math.Ceiling(
                        totalCases /
                        (double)pageSize
                    )
                );


            if (page > pageCount)
            {
                page = pageCount;
            }


            var pagedItems =
                items
                    .Skip(
                        (page - 1) *
                        pageSize
                    )
                    .Take(pageSize)
                    .ToList();


            // =================================================
            // VIEW MODEL
            // =================================================

            var model =
                new DashboardViewModel
                {
                    IsSupervisor =
                        isSupervisor,

                    IsExpert =
                        isExpert,


                    // KPI
                    TotalCases =
                        totalCases,

                    AssignedCount =
                        assignedCount,

                    InProgressCount =
                        inProgressCount,

                    ChallengeCount =
                        challengeCount,

                    CompletionPendingCount =
                        completionPendingCount,

                    DelayedCount =
                        delayedCount,

                    CompletedCount =
                        completedCount,


                    // PRIORITY
                    P1Count =
                        items.Count(
                            x =>
                                x.Priority == "P1"
                        ),

                    P2Count =
                        items.Count(
                            x =>
                                x.Priority == "P2"
                        ),

                    P3Count =
                        items.Count(
                            x =>
                                x.Priority == "P3"
                        ),

                    P4Count =
                        items.Count(
                            x =>
                                x.Priority == "P4"
                        ),


                    // SLA
                    OnTrackCount =
                        onTrackCount,

                    CompliancePercentage =
                        compliancePercentage,


                    // FILTERS
                    FromDate =
                        fromDate,

                    ToDate =
                        toDate,

                    SelectedExpertId =
                        effectiveExpertId,

                    SelectedStage =
                        stage,

                    SelectedPriority =
                        priority,

                    SelectedSlaStatus =
                        slaStatus,

                    SelectedFilter =
                        "All",


                    // EXPERT OPTIONS
                    Experts =
                        expertUsers
                            .Select(
                                x =>
                                    new DashboardExpertOptionViewModel
                                    {
                                        Id =
                                            x.Id,

                                        FullName =
                                            x.FullName,

                                        EmployeeNumber =
                                            x.EmployeeNumber
                                    }
                            )
                            .ToList(),


                    ExpertWorkload =
                        workload,


                    // TABLE
                    Cases =
                        pagedItems,


                    // PAGINATION
                    CurrentPage =
                        page,

                    PageSize =
                        pageSize,

                    PageCount =
                        pageCount
                };


            return View(model);
        }
    }
}