using System.ComponentModel.DataAnnotations;

namespace CaseManagementSystem.ViewModels
{
    public class CompletionViewModel
    {
        [Required]
        public int CaseId { get; set; }

        [Required]
        [StringLength(2000)]
        public string CompletionSummary { get; set; } = string.Empty;

        public string ReturnReason { get; set; } = string.Empty;
    }
}