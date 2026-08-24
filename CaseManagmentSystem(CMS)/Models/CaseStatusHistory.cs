using CaseManagementSystem.Models;


namespace CaseManagementSystem.Models
{
    public class CaseStatusHistory
    {
        public int Id { get; set; }
        public int CaseId { get; set; }
        public string? PerformedByUserId { get; set; }
        public string Action { get; set; } = string.Empty;
        public int? PreviousStageId { get; set; }
        public int? NewStageId { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }

        public Case? Case { get; set; }
        public ApplicationUser? PerformedByUser { get; set; }
        public WorkflowStage? PreviousStage { get; set; }
        public WorkflowStage? NewStage { get; set; }
    }
}
