namespace CaseManagementSystem.ViewModels
{
    public class DashboardViewModel
    {
        // =====================================================
        // ROLE
        // =====================================================

        public bool IsSupervisor { get; set; }

        public bool IsExpert { get; set; }


        // =====================================================
        // KPI
        // =====================================================

        public int TotalCases { get; set; }

        public int AssignedCount { get; set; }

        public int InProgressCount { get; set; }

        public int ChallengeCount { get; set; }

        public int CompletionPendingCount { get; set; }

        public int DelayedCount { get; set; }

        public int CompletedCount { get; set; }


        // =====================================================
        // PRIORITY
        // =====================================================

        public int P1Count { get; set; }

        public int P2Count { get; set; }

        public int P3Count { get; set; }

        public int P4Count { get; set; }


        // =====================================================
        // SLA
        // =====================================================

        public int OnTrackCount { get; set; }

        public double CompliancePercentage { get; set; }


        // =====================================================
        // FILTERS
        // =====================================================

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public string SelectedExpertId { get; set; } = string.Empty;

        public string SelectedStage { get; set; } = "All";

        public string SelectedPriority { get; set; } = "All";

        public string SelectedSlaStatus { get; set; } = "All";


        // Retained for compatibility with older Home/Dashboard code.
        public string SelectedFilter { get; set; } = "All";


        // =====================================================
        // EXPERTS
        // =====================================================

        public List<DashboardExpertOptionViewModel> Experts { get; set; }
            = new();

        public List<DashboardExpertWorkloadViewModel> ExpertWorkload { get; set; }
            = new();


        // =====================================================
        // CASE TABLE
        // =====================================================

        public List<DashboardCaseItemViewModel> Cases { get; set; }
            = new();


        // =====================================================
        // PAGINATION
        // =====================================================

        public int CurrentPage { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public int PageCount { get; set; } = 1;
    }


    public class DashboardExpertOptionViewModel
    {
        public string Id { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string EmployeeNumber { get; set; } = string.Empty;
    }


    public class DashboardExpertWorkloadViewModel
    {
        public string ExpertId { get; set; } = string.Empty;

        public string ExpertName { get; set; } = string.Empty;

        public string EmployeeNumber { get; set; } = string.Empty;

        public int ActiveCount { get; set; }

        public int AssignedCount { get; set; }

        public int InProgressCount { get; set; }

        public int ChallengeCount { get; set; }

        public int CompletionPendingCount { get; set; }

        public int CompletedCount { get; set; }

        public int DelayedCount { get; set; }
    }
}