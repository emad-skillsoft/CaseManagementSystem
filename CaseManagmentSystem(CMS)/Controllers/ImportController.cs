using CaseManagementSystem.Services;
using CaseManagementSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;

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
    }
}
