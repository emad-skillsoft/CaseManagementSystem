using CaseManagementSystem.Constants;
using CaseManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CaseManagementSystem.Controllers
{
    [Authorize]
    public class CasesController : Controller
    {
        private readonly ICaseService _caseService;

        public CasesController(ICaseService caseService)
        {
            _caseService = caseService;
        }

        [Authorize(Roles = RoleNames.Supervisor)]
        public async Task<IActionResult> Index()
        {
            return View(await _caseService.GetAllCasesAsync());
        }

        [Authorize(Roles = RoleNames.Expert)]
        public async Task<IActionResult> MyCases()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            return View(await _caseService.GetMyCasesAsync(userId));
        }

        public async Task<IActionResult> Details(int id)
        {
            var model = await _caseService.GetCaseDetailsAsync(id);

            if (model == null)
                return NotFound();

            if (User.IsInRole(RoleNames.Expert))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (model.AssignedExpertId != userId)
                    return Forbid();
            }

            return View(model);
        }
    }
}
