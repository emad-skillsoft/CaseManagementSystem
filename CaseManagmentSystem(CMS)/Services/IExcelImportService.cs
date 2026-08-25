
using CaseManagmentSystem_CMS_.Dtos;
using Microsoft.AspNetCore.Http;

namespace CaseManagementSystem.Services
{
    public interface IExcelImportService
    {
        Task<(bool IsValid, string ErrorMessage)> ValidateFileAsync(IFormFile file);
        Task<List<ExcelCaseRowDto>> ReadRowsAsync(IFormFile file);
    }
}
