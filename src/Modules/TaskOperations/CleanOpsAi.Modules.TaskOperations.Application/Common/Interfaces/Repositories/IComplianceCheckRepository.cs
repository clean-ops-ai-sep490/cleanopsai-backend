using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.Modules.TaskOperations.Domain.Entities;
using CleanOpsAi.Modules.TaskOperations.Domain.Enums;

namespace CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Repositories
{
    public interface IComplianceCheckRepository : IBaseRepo<ComplianceCheck, Guid>
    {
        Task<ComplianceCheck?> GetByExecutionIdAndTypeAsync(
            Guid taskStepExecutionId,
            ComplianceCheckType type,
            CancellationToken ct = default);

        Task<PaginatedResult<ComplianceCheck>> GetPendingSupervisorChecksAsync(
            Guid supervisorId,
            PaginationRequest request,
            CancellationToken ct = default);
	}
}
