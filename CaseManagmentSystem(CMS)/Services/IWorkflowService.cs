namespace CaseManagementSystem.Services
{
    public interface IWorkflowService
    {
        Task<bool> ChangeStageAsync(int caseId, string newStageName, string performedByUserId, string? comment = null);
        Task<bool> StartChallengeAsync(int caseId, string userId, string reason);
        Task<bool> ResolveChallengeAsync(int caseId, string supervisorUserId, string comment);
        Task<bool> SubmitCompletionAsync(int caseId, string userId, string summary);
        Task<bool> ApproveCompletionAsync(int caseId, string supervisorUserId);
        Task<bool> ReturnToWorkAsync(int caseId, string supervisorUserId, string reason);
    }
}
