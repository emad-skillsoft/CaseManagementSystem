using System.ComponentModel.DataAnnotations;

namespace CaseManagementSystem.ViewModels
{
    public class ChallengeViewModel
    {
        [Required]
        public int CaseId { get; set; }

        public string Reason { get; set; } = string.Empty;
        public string SupervisorComment { get; set; } = string.Empty;
    }
}

