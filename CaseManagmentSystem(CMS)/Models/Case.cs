using CaseManagementSystem.Enums;

namespace CaseManagementSystem.Models
{
    public class Case
    {
        public int Id { get; set; }
        public string ExternalCaseId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public CasePriority Priority { get; set; }
        public DateTime SLAStartDate { get; set; }
        public DateTime ImportedAt { get; set; }
        public string AssignedExpertId { get; set; } = string.Empty;
        public int CurrentWorkflowStageId { get; set; }
        public DateTime? CompletedAt { get; set; }

        public ApplicationUser? AssignedExpert { get; set; }
        public WorkflowStage? CurrentWorkflowStage { get; set; }
        public ICollection<CaseStatusHistory> History { get; set; } = new List<CaseStatusHistory>();
    }
}
