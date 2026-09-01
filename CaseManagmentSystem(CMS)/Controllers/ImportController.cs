using CaseManagementSystem.Services;
using CaseManagementSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CaseManagementSystem.Controllers
{
    public class ImportController : Controller
    {
        private readonly IExcelImportService _excelImportService;

        public ImportController(IExcelImportService excelImportService)
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
        public async Task<IActionResult> Preview(ExcelImportViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Index", model);

            var validation = await _excelImportService.ValidateFileAsync(model.ExcelFile!);

            if (!validation.IsValid)
            {
                ModelState.AddModelError(nameof(model.ExcelFile), validation.ErrorMessage);
                return View("Index", model);
            }

            model.Rows = await _excelImportService.ReadRowsAsync(model.ExcelFile!);
            return View("Index", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(ExcelImportViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Index", model);

            var validation = await _excelImportService.ValidateFileAsync(model.ExcelFile!);

            if (!validation.IsValid)
            {
                ModelState.AddModelError(nameof(model.ExcelFile), validation.ErrorMessage);
                return View("Index", model);
            }

            model.Rows = await _excelImportService.ReadRowsAsync(model.ExcelFile!);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? string.Empty;

            var imported = await _excelImportService
                .ImportCasesAsync(model.Rows, userId);

            ViewBag.Message = $"Imported {imported} case(s).";
            return View("Index", model);
        }
        [HttpGet]
        public async Task<IActionResult> Review()
        {
            return View(await _excelImportService.GetReviewItemsAsync());
        }


    }
}
