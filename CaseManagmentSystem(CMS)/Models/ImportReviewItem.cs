namespace CaseManagementSystem.Models
{
    public class ImportReviewItem
    {
        public int Id { get; set; }
        public string ExternalCaseId { get; set; } = string.Empty;
        public string EmployeeNumber { get; set; } = string.Empty;
        public string Issue { get; set; } = string.Empty;
        public int RowNumber { get; set; }
        public bool IsResolved { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
