using CaseManagementSystem.Enums;

namespace CaseManagementSystem.ViewModels
{
    public class CaseDetailsViewModel
    {
        public int Id { get; set; }
        public string ExternalCaseId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public CasePriority Priority { get; set; }
        public DateTime SLAStartDate { get; set; }
        public DateTime ImportedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string AssignedExpertId { get; set; } = string.Empty;
        public string AssignedExpertName { get; set; } = string.Empty;
        public string CurrentStage { get; set; } = string.Empty;
        public List<CaseHistoryItemViewModel> History { get; set; } = new();
        public List<CaseUpdateItemViewModel> Updates { get; set; } = new();

    }
    public class CaseUpdateItemViewModel
    {
        public string UserName { get; set; } = string.Empty;
        public string UpdateText { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }


    public class CaseHistoryItemViewModel
    {
        public string Action { get; set; } = string.Empty;
        public string? PreviousStage { get; set; }
        public string? NewStage { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
