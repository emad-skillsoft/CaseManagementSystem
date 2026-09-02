
using CaseManagementSystem.Dtos;
using CaseManagementSystem.Models;
using Microsoft.AspNetCore.Http;

namespace CaseManagementSystem.Services
{
    public interface IExcelImportService
    {
        Task<(bool IsValid, string ErrorMessage)> ValidateFileAsync(IFormFile file);
        Task<List<ExcelCaseRowDto>> ReadRowsAsync(IFormFile file);
        Task<ImportResultDto> ImportCasesAsync(
    List<ExcelCaseRowDto> rows,
    string performedByUserId);

        Task<List<ImportReviewItem>> GetReviewItemsAsync();
        Task<bool> ResolveReviewItemAsync(
    int reviewItemId,
    string employeeNumber,
    string performedByUserId);

    }
}
