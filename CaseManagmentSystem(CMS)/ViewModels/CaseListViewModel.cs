using CaseManagementSystem.Enums;

namespace CaseManagementSystem.ViewModels
{
    public class CaseListViewModel
    {
        public int Id { get; set; }
        public string ExternalCaseId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public CasePriority Priority { get; set; }
        public string CurrentStage { get; set; } = string.Empty;
        public string AssignedExpertName { get; set; } = string.Empty;
        public DateTime SLAStartDate { get; set; }
    }
}

