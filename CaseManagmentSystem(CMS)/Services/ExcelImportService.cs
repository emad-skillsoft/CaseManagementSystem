
using CaseManagmentSystem_CMS_.Dtos;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;

namespace CaseManagementSystem.Services
{
    public class ExcelImportService : IExcelImportService
    {
        public Task<(bool IsValid, string ErrorMessage)> ValidateFileAsync(IFormFile file)
        {
            if (file == null)
                return Task.FromResult((false, "No file was selected."));

            if (file.Length == 0)
                return Task.FromResult((false, "The selected file is empty."));

            var extension = Path.GetExtension(file.FileName);
            if (!extension.Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
                return Task.FromResult((false, "Only .xlsx files are allowed."));

            const long maxFileSize = 5 * 1024 * 1024;
            if (file.Length > maxFileSize)
                return Task.FromResult((false, "The file size must not exceed 5 MB."));

            try
            {
                using var stream = file.OpenReadStream();
                using var workbook = new XLWorkbook(stream);

                if (!workbook.Worksheets.Any())
                    return Task.FromResult((false, "The Excel file does not contain a worksheet."));

                return Task.FromResult((true, string.Empty));
            }
            catch
            {
                return Task.FromResult((false, "The Excel file could not be read."));
            }
        }

        public Task<List<ExcelCaseRowDto>> ReadRowsAsync(IFormFile file)
        {
            // Session 2 replaces this temporary implementation.
            return Task.FromResult(new List<ExcelCaseRowDto>());
        }
    }
}
