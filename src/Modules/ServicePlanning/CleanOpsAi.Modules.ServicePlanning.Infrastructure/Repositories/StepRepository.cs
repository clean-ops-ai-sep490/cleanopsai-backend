using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.BuildingBlocks.Infrastructure.Extensions;
using CleanOpsAi.Modules.ServicePlanning.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.ServicePlanning.Domain.Entities;
using CleanOpsAi.Modules.ServicePlanning.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CleanOpsAi.Modules.ServicePlanning.Infrastructure.Repositories
{
	public class StepRepository : BaseRepo<Step, Guid>, IStepRepository
	{
		public StepRepository(ServicePlanningDbContext context) : base(context)
		{
		}

		public async Task<List<Step>> GetByIdsAsync(List<Guid> ids, CancellationToken cancellationToken = default)
		{
			return await _context.Steps
				.Where(s => ids.Contains(s.Id))
				.ToListAsync(cancellationToken);
		}

		public async Task<PaginatedResult<Step>> GetsPaging(PaginationRequest request, CancellationToken ct = default)
		{
			return await _context.Steps.ToPaginatedResultAsync(request, ct);
		}

		public async Task<bool> IsActionKeyExists(string actionKey, CancellationToken ct = default)
		{
			var existsStep = await _context.Steps.FirstOrDefaultAsync(s => s.ActionKey == actionKey, ct);
			if (existsStep != null)
			{
				return true;
			}
			return false;
		}
	}
}
