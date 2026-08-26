using System.ComponentModel.DataAnnotations;

namespace CaseManagementSystem.ViewModels
{
    public class CaseUpdateViewModel
    {
        [Required]
        public int CaseId { get; set; }

        [Required, MinLength(3)]
        public string UpdateText { get; set; } = string.Empty;
    }
}

