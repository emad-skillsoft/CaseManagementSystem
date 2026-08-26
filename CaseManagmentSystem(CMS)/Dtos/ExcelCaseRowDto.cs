namespace CaseManagementSystem.Dtos
{
    public class ExcelCaseRowDto
    {
        public int RowNumber { get; set; }
        public string ExternalCaseId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime? SLAStartDate { get; set; }
        public string Priority { get; set; } = string.Empty;
        public string AssignedEmployeeNumber { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new();
        public bool IsValid => Errors.Count == 0;

    }
}
