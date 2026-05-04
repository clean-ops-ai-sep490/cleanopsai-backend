using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.BuildingBlocks.Infrastructure.Extensions;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.TaskOperations.Domain.Entities;
using CleanOpsAi.Modules.TaskOperations.Domain.Enums;
using CleanOpsAi.Modules.TaskOperations.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CleanOpsAi.Modules.TaskOperations.Infrastructure.Repositories
{
    public class ComplianceCheckRepository : BaseRepo<ComplianceCheck, Guid>, IComplianceCheckRepository
	{ 

        public ComplianceCheckRepository(TaskOperationsDbContext context) : base(context)
		{
             
        }
         
        public async Task<ComplianceCheck?> GetByExecutionIdAndTypeAsync(
            Guid taskStepExecutionId,
            ComplianceCheckType type,
            CancellationToken ct = default)
        {
			return await _context.ComplianceChecks
				.Where(x => x.TaskStepExecutionId == taskStepExecutionId && x.Type == type)
				.OrderByDescending(x => x.Id)  
				.FirstOrDefaultAsync(ct);
		}

		public async Task<PaginatedResult<ComplianceCheck>> GetPendingSupervisorChecksAsync(
        Guid supervisorId,
		PaginationRequest request,
	    CancellationToken ct = default)
		{
			return await _context.ComplianceChecks
				.Where(x => x.SupervisorId == supervisorId
						 && x.Status == ComplianceCheckStatus.PendingSupervisor)
				.OrderByDescending(x => x.Created)
				.ToPaginatedResultAsync(request, ct);
		}
	}
}
