using CaseManagementSystem.Constants;
using CaseManagementSystem.Services;
using CaseManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CaseManagementSystem.Controllers
{
    [Authorize]
    public class CasesController : Controller
    {
        private readonly ICaseService _caseService;
        private readonly IWorkflowService _workflowService;

        public CasesController(
            ICaseService caseService,
            IWorkflowService workflowService)
        {
            _caseService = caseService;
            _workflowService = workflowService;
        }

        [Authorize(Roles = RoleNames.Expert)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUpdate(CaseUpdateViewModel model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction(
                    nameof(Details),
                    new { id = model.CaseId });

            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var success = await _caseService
                .AddUpdateAsync(
                    model.CaseId,
                    userId,
                    model.UpdateText);

            if (!success)
                return Forbid();

            return RedirectToAction(
                nameof(Details),
                new { id = model.CaseId });
        }

        [Authorize(Roles = RoleNames.Supervisor)]
        public async Task<IActionResult> Index()
        {
            return View(
                await _caseService.GetAllCasesAsync());
        }

        [Authorize(Roles = RoleNames.Expert)]
        public async Task<IActionResult> MyCases()
        {
            var userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier)!;

            return View(
                await _caseService
                    .GetMyCasesAsync(userId));
        }

        public async Task<IActionResult> Details(int id)
        {
            var model =
                await _caseService
                    .GetCaseDetailsAsync(id);

            if (model == null)
                return NotFound();

            if (User.IsInRole(RoleNames.Expert))
            {
                var userId =
                    User.FindFirstValue(
                        ClaimTypes.NameIdentifier);

                if (model.AssignedExpertId != userId)
                    return Forbid();
            }

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = RoleNames.Expert)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitCompletion(
            CompletionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["CaseError"] =
                    "Completion summary is required.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = model.CaseId });
            }

            var userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier)!;

            var success =
                await _workflowService
                    .SubmitCompletionAsync(
                        model.CaseId,
                        userId,
                        model.CompletionSummary);

            TempData[
                success
                    ? "CaseSuccess"
                    : "CaseError"] =
                success
                    ? "Completion request submitted."
                    : "Completion request could not be submitted.";

            return RedirectToAction(
                nameof(Details),
                new { id = model.CaseId });
        }

        [HttpPost]
        [Authorize(Roles = RoleNames.Supervisor)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveCompletion(
            int caseId)
        {
            var userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier)!;

            var success =
                await _workflowService
                    .ApproveCompletionAsync(
                        caseId,
                        userId);

            TempData[
                success
                    ? "CaseSuccess"
                    : "CaseError"] =
                success
                    ? "Completion approved."
                    : "Completion could not be approved.";

            return RedirectToAction(
                nameof(Details),
                new { id = caseId });
        }

        [HttpPost]
        [Authorize(Roles = RoleNames.Supervisor)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReturnToWork(
            int caseId,
            string reason)
        {
            var userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier)!;

            var success =
                await _workflowService
                    .ReturnToWorkAsync(
                        caseId,
                        userId,
                        reason);

            TempData[
                success
                    ? "CaseSuccess"
                    : "CaseError"] =
                success
                    ? "Case returned to the Expert."
                    : "Return failed. A reason is required and the Case must be Completion Pending.";

            return RedirectToAction(
                nameof(Details),
                new { id = caseId });
        }
    }
}