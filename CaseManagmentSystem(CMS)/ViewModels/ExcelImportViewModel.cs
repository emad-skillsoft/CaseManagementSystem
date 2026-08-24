using CaseManagmentSystem_CMS_.Dtos;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace CaseManagementSystem.ViewModels
{
    public class ExcelImportViewModel
    {
        [Required(ErrorMessage = "Please select an Excel file.")]
        public IFormFile? ExcelFile { get; set; }

        public List<ExcelCaseRowDto> Rows { get; set; } = new();
    }
}
