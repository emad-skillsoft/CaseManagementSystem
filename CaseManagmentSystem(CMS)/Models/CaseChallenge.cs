namespace CaseManagementSystem.Models
{
    public class CaseChallenge
    {
        public int Id { get; set; }
        public int CaseId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string StartedByUserId { get; set; } = string.Empty;
        public DateTime StartedAt { get; set; }
        public string? ResolvedByUserId { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? SupervisorComment { get; set; }

        public Case? Case { get; set; }
        public ApplicationUser? StartedByUser { get; set; }
        public ApplicationUser? ResolvedByUser { get; set; }
    }
}
