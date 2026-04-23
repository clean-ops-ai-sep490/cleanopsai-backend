using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Response;

namespace CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services
{ 
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

		Task<SupervisorCheckDetailDto> GetSupervisorCheckDetailAsync( Guid complianceCheckId, CancellationToken ct = default);

        Task ApplySupervisorReviewAsync(
            Guid complianceCheckId,
            SupervisorReviewRequest request,
            CancellationToken ct = default);
	}
}
