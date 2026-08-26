namespace CaseManagementSystem.Models
{
    public class CaseUpdate
    {
        public int Id { get; set; }
        public int CaseId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UpdateText { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public Case? Case { get; set; }
        public ApplicationUser? User { get; set; }
    }
}
