namespace CaseManagementSystem.ViewModels
{
    public class SLAStatusViewModel
    {
        public DateTime SLAStartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public TimeSpan TotalDuration { get; set; }
        public TimeSpan InProgressDuration { get; set; }
        public TimeSpan ChallengeDuration { get; set; }
        public TimeSpan RemainingTime { get; set; }
        public bool IsDelayed { get; set; }
    }
}
