
using CaseManagementSystem.Dtos;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;

namespace CaseManagementSystem.Services
{
    public class ExcelImportService : IExcelImportService
    {
        private static readonly string[] ExpectedHeaders =
        {
            "ExternalCaseId",
            "Title",
            "Description",
            "SLAStartDate",
            "Priority",
            "AssignedEmployeeNumber"
        };

        private static readonly HashSet<string> AllowedPriorities =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "P1", "P2", "P3", "P4"
            };

        public Task<(bool IsValid, string ErrorMessage)> ValidateFileAsync(IFormFile file)
        {
            if (file == null)
                return Task.FromResult((false, "No file was selected."));

            if (file.Length == 0)
                return Task.FromResult((false, "The selected file is empty."));

            if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
                return Task.FromResult((false, "Only .xlsx files are allowed."));

            if (file.Length > 5 * 1024 * 1024)
                return Task.FromResult((false, "The file size must not exceed 5 MB."));

            try
            {
                using var stream = file.OpenReadStream();
                using var workbook = new XLWorkbook(stream);

                if (!workbook.Worksheets.Any())
                    return Task.FromResult((false, "The Excel file does not contain a worksheet."));

                var sheet = workbook.Worksheets.First();

                for (var col = 1; col <= ExpectedHeaders.Length; col++)
                {
                    var actual = sheet.Cell(1, col).GetString().Trim();
                    var expected = ExpectedHeaders[col - 1];

                    if (!actual.Equals(expected, StringComparison.OrdinalIgnoreCase))
                    {
                        return Task.FromResult((
                            false,
                            $"Invalid Excel format. Column {col} must be '{expected}'."));
                    }
                }

                return Task.FromResult((true, string.Empty));
            }
            catch
            {
                return Task.FromResult((false, "The Excel file could not be read."));
            }
        }

        public Task<List<ExcelCaseRowDto>> ReadRowsAsync(IFormFile file)
        {
            var result = new List<ExcelCaseRowDto>();

            using var stream = file.OpenReadStream();
            using var workbook = new XLWorkbook(stream);
            var sheet = workbook.Worksheets.First();

            foreach (var row in sheet.RowsUsed().Skip(1))
            {
                var rawValues = Enumerable.Range(1, 6)
                    .Select(i => row.Cell(i).GetFormattedString().Trim())
                    .ToArray();

                if (rawValues.All(string.IsNullOrWhiteSpace))
                    continue;

                var item = new ExcelCaseRowDto
                {
                    RowNumber = row.RowNumber(),
                    ExternalCaseId = row.Cell(1).GetString().Trim(),
                    Title = row.Cell(2).GetString().Trim(),
                    Description = row.Cell(3).GetString().Trim(),
                    Priority = row.Cell(5).GetString().Trim().ToUpperInvariant(),
                    AssignedEmployeeNumber = row.Cell(6).GetString().Trim()
                };

                if (row.Cell(4).TryGetValue<DateTime>(out var excelDate))
                {
                    item.SLAStartDate = excelDate;
                }
                else if (DateTime.TryParse(row.Cell(4).GetFormattedString(), out var parsedDate))
                {
                    item.SLAStartDate = parsedDate;
                }

                if (string.IsNullOrWhiteSpace(item.ExternalCaseId))
                    item.Errors.Add("ExternalCaseId is required.");

                if (string.IsNullOrWhiteSpace(item.Title))
                    item.Errors.Add("Title is required.");

                if (string.IsNullOrWhiteSpace(item.Description))
                    item.Errors.Add("Description is required.");

                if (item.SLAStartDate == null)
                    item.Errors.Add("SLA Start Date is invalid.");

                if (!AllowedPriorities.Contains(item.Priority))
                    item.Errors.Add("Priority must be P1, P2, P3, or P4.");

                if (string.IsNullOrWhiteSpace(item.AssignedEmployeeNumber))
                    item.Errors.Add("Assigned Employee Number is required.");

                result.Add(item);
            }

            return Task.FromResult(result);
        }
    }
}
