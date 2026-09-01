namespace CaseManagementSystem.Dtos
{
    public class ImportResultDto
    {
        public int TotalRows { get; set; }
        public int Imported { get; set; }
        public int NeedsReview { get; set; }
        public int Invalid { get; set; }
        public int Duplicates { get; set; }
    }
}
