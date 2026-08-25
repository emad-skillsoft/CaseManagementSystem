namespace CaseManagementSystem.Enums
{
    public enum CasePriority { P1, P2, P3, P4 }

    public static class RoleNames
    {
        public const string Supervisor = "Supervisor";
        public const string Expert = "Expert";
    }

    public static class WorkflowStageNames
    {
        public const string Assigned = "Assigned";
        public const string InProgress = "InProgress";
        public const string Challenge = "Challenge";
        public const string CompletionPending = "CompletionPending";
        public const string Completed = "Completed";

    }
}
