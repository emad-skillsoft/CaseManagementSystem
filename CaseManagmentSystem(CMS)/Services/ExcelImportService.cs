
using CaseManagementSystem.Dtos;
using ClosedXML.Excel;
using CaseManagementSystem.Constants;
using CaseManagementSystem.Data;
using CaseManagementSystem.Enums;
using CaseManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

using WorkflowStageNames = CaseManagementSystem.Constants.WorkflowStageNames;

namespace CaseManagementSystem.Services
{
    public class ExcelImportService : IExcelImportService
    {
        private readonly ApplicationDbContext _db;
        private readonly IUserService _userService;
        public ExcelImportService(ApplicationDbContext db, IUserService userService)
        {
            _db = db;
            _userService = userService;
        }


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

        public async Task<ImportResultDto> ImportCasesAsync(
         List<ExcelCaseRowDto> rows,
         string performedByUserId)
        {
            var result = new ImportResultDto
            {
                TotalRows = rows.Count
            };

            var assignedStage = await _db.WorkflowStages
                .FirstAsync(x => x.Name == WorkflowStageNames.Assigned);

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var row in rows)
            {
                if (!row.IsValid)
                {
                    result.Invalid++;
                    continue;
                }

                if (!seen.Add(row.ExternalCaseId))
                {
                    row.Errors.Add("Duplicate ExternalCaseId inside this Excel file.");
                    result.Duplicates++;
                    continue;
                }

                var existsInDatabase = await _db.Cases
                    .AnyAsync(x => x.ExternalCaseId == row.ExternalCaseId);

                if (existsInDatabase)
                {
                    row.Errors.Add("ExternalCaseId already exists in the database.");
                    result.Duplicates++;
                    continue;
                }

                var expert = await _userService
                    .GetExpertByEmployeeNumberAsync(row.AssignedEmployeeNumber);

                if (expert == null)
                {
                    row.Errors.Add("Expert was not found. Row sent to Needs Review.");

                    Enum.TryParse<CasePriority>(
                        row.Priority,
                        true,
                        out var reviewPriority);

                    _db.ImportReviewItems.Add(new ImportReviewItem
                    {
                        ExternalCaseId = row.ExternalCaseId,
                        Title = row.Title,
                        Description = row.Description,
                        Priority = reviewPriority,
                        SLAStartDate = row.SLAStartDate!.Value,
                        EmployeeNumber = row.AssignedEmployeeNumber,
                        Issue = "Expert was not found for EmployeeNumber.",
                        RowNumber = row.RowNumber,
                        IsResolved = false,
                        CreatedAt = DateTime.UtcNow
                    });

                    await _db.SaveChangesAsync();
                    result.NeedsReview++;
                    continue;
                }

                Enum.TryParse<CasePriority>(
                    row.Priority,
                    true,
                    out var priority);

                var caseItem = new Case
                {
                    ExternalCaseId = row.ExternalCaseId,
                    Title = row.Title,
                    Description = row.Description,
                    Priority = priority,
                    SLAStartDate = row.SLAStartDate!.Value,
                    ImportedAt = DateTime.UtcNow,
                    AssignedExpertId = expert.Id,
                    CurrentWorkflowStageId = assignedStage.Id
                };

                caseItem.History.Add(new CaseStatusHistory
                {
                    PerformedByUserId = performedByUserId,
                    Action = "Imported and automatically assigned",
                    NewStageId = assignedStage.Id,
                    Comment = $"Assigned using EmployeeNumber {row.AssignedEmployeeNumber}",
                    CreatedAt = DateTime.UtcNow
                });

                _db.Cases.Add(caseItem);
                await _db.SaveChangesAsync();
                result.Imported++;
            }

            return result;
        }



        public Task<List<ImportReviewItem>> GetReviewItemsAsync()
        {
            return _db.ImportReviewItems
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }
        public async Task<bool> ResolveReviewItemAsync(
    int reviewItemId,
    string employeeNumber,
    string performedByUserId)
        {
            if (string.IsNullOrWhiteSpace(employeeNumber))
                return false;

            var reviewItem = await _db.ImportReviewItems
                .FirstOrDefaultAsync(x =>
                    x.Id == reviewItemId &&
                    !x.IsResolved);

            if (reviewItem == null)
                return false;

            var duplicateCase = await _db.Cases
                .AnyAsync(x => x.ExternalCaseId == reviewItem.ExternalCaseId);

            if (duplicateCase)
                return false;

            var expert = await _userService
                .GetExpertByEmployeeNumberAsync(employeeNumber.Trim());

            if (expert == null)
                return false;

            var assignedStage = await _db.WorkflowStages
                .FirstAsync(x => x.Name == WorkflowStageNames.Assigned);

            var caseItem = new Case
            {
                ExternalCaseId = reviewItem.ExternalCaseId,
                Title = reviewItem.Title,
                Description = reviewItem.Description,
                Priority = reviewItem.Priority,
                SLAStartDate = reviewItem.SLAStartDate,
                ImportedAt = DateTime.UtcNow,
                AssignedExpertId = expert.Id,
                CurrentWorkflowStageId = assignedStage.Id
            };

            caseItem.History.Add(new CaseStatusHistory
            {
                PerformedByUserId = performedByUserId,
                Action = "Import Review resolved and Case assigned",
                NewStageId = assignedStage.Id,
                Comment = $"EmployeeNumber corrected to {employeeNumber.Trim()}",
                CreatedAt = DateTime.UtcNow
            });

            reviewItem.EmployeeNumber = employeeNumber.Trim();
            reviewItem.Issue = "Resolved";
            reviewItem.IsResolved = true;

            _db.Cases.Add(caseItem);
            await _db.SaveChangesAsync();

            return true;
        }

    }
}
