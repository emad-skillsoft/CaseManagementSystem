using System.ComponentModel.DataAnnotations;

namespace CaseManagementSystem.ViewModels
{
    public class ImportReviewViewModel
    {
        [Required]
        public int ReviewItemId { get; set; }

        [Required]
        public string EmployeeNumber { get; set; } = string.Empty;
    }
}
