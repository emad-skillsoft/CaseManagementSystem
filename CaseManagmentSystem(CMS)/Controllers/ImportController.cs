using CaseManagementSystem.Constants;
using CaseManagementSystem.Services;
using CaseManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CaseManagementSystem.Controllers
{
    [Authorize(Roles = RoleNames.Supervisor)]
    public class ImportController : Controller
    {
        private readonly IExcelImportService _excelImportService;

        public ImportController(
            IExcelImportService excelImportService)
        {
            _excelImportService = excelImportService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new ExcelImportViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Preview(
            ExcelImportViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Index", model);

            var validation =
                await _excelImportService
                    .ValidateFileAsync(model.ExcelFile!);

            if (!validation.IsValid)
            {
                ModelState.AddModelError(
                    nameof(model.ExcelFile),
                    validation.ErrorMessage);

                return View("Index", model);
            }

            try
            {
                model.Rows =
                    await _excelImportService
                        .ReadRowsAsync(model.ExcelFile!);
            }
            catch
            {
                ModelState.AddModelError(
                    nameof(model.ExcelFile),
                    "The Excel file could not be processed.");

                return View("Index", model);
            }

            return View("Index", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(
            ExcelImportViewModel model)
        {
            if (!ModelState.IsValid ||
                model.ExcelFile == null)
            {
                return View("Index", model);
            }

            var validation =
                await _excelImportService
                    .ValidateFileAsync(model.ExcelFile);

            if (!validation.IsValid)
            {
                ModelState.AddModelError(
                    nameof(model.ExcelFile),
                    validation.ErrorMessage);

                return View("Index", model);
            }

            try
            {
                model.Rows =
                    await _excelImportService
                        .ReadRowsAsync(model.ExcelFile);
            }
            catch
            {
                ModelState.AddModelError(
                    nameof(model.ExcelFile),
                    "The Excel file could not be processed.");

                return View("Index", model);
            }

            var userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier)!;

            model.Result =
                await _excelImportService
                    .ImportCasesAsync(
                        model.Rows,
                        userId);

            return View("Index", model);
        }

        [HttpGet]
        public async Task<IActionResult> Review()
        {
            return View(
                await _excelImportService
                    .GetReviewItemsAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResolveReview(
            ImportReviewViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ReviewError"] =
                    "Employee Number is required.";

                return RedirectToAction(
                    nameof(Review));
            }

            var userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier)!;

            var success =
                await _excelImportService
                    .ResolveReviewItemAsync(
                        model.ReviewItemId,
                        model.EmployeeNumber,
                        userId);

            TempData[
                success
                    ? "ReviewSuccess"
                    : "ReviewError"] =
                success
                    ? "Review item resolved and Case created."
                    : "Review item could not be resolved. Check the Employee Number or duplicate Case.";

            return RedirectToAction(
                nameof(Review));
        }
    }
}