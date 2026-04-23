using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Response;

namespace CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services
{
    /// <summary>
    /// Orchestrates the full lifecycle of AI-automated <c>ComplianceCheck</c> records:
    /// initiation, scoring submission, result application, and real-time notification.
    /// </summary>
    public interface IComplianceCheckService
    {  
        Task<InitiateAiCheckResult> InitiateAiCheckAsync(
            Guid taskStepExecutionId,
            CancellationToken ct = default);

        Task ApplyScoringResultsAsync(
			ScoringCompletedEvent evt,
            CancellationToken ct = default);

        
        Task<ComplianceCheckStatusResponse?> GetCurrentStatusAsync(
            Guid taskStepExecutionId,
            CancellationToken ct = default);

        Task<PaginatedResult<PendingSupervisorCheckDto>> GetPendingSupervisorChecksAsync(
            PaginationRequest request,
            CancellationToken ct = default);


	}
}
