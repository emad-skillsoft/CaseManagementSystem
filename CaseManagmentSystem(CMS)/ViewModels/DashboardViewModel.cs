namespace CaseManagementSystem.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalCases { get; set; }
        public int InProgressCount { get; set; }
        public int ChallengeCount { get; set; }
        public int DelayedCount { get; set; }
        public int CompletedCount { get; set; }

        public string SelectedFilter { get; set; } = "All";

        public List<DashboardCaseItemViewModel> Cases { get; set; }
            = new();
    }
}
