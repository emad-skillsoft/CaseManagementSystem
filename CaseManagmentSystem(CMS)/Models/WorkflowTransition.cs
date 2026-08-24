using CaseManagementSystem.Models;


namespace CaseManagementSystem.Models
{
    public class WorkflowTransition
    {
        public int Id { get; set; }
        public int FromStageId { get; set; }
        public int ToStageId { get; set; }
        public WorkflowStage? FromStage { get; set; }
        public WorkflowStage? ToStage { get; set; }
    }
}
