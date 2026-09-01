namespace CaseManagementSystem.ViewModels
{
    public class DashboardCaseItemViewModel
    {
        public int Id { get; set; }
        public string ExternalCaseId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string CurrentStage { get; set; } = string.Empty;
        public string AssignedExpert { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public bool IsDelayed { get; set; }
    }
}

