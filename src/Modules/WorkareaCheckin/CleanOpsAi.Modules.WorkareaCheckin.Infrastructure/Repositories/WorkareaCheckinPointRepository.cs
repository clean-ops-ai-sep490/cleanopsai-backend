using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.BuildingBlocks.Infrastructure.Extensions;
using CleanOpsAi.Modules.WorkareaCheckin.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.WorkareaCheckin.Application.DTOs.Request;
using CleanOpsAi.Modules.WorkareaCheckin.Domain.Entities;
using CleanOpsAi.Modules.WorkareaCheckin.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CleanOpsAi.Modules.WorkareaCheckin.Infrastructure.Repositories
{
	public class WorkareaCheckinPointRepository : BaseRepo<WorkareaCheckinPoint, Guid>, IWorkareaCheckinPointRepository
	{
		public WorkareaCheckinPointRepository(WorkareaCheckinDbContext context) : base(context)
		{

		}

		public async Task<int> CountByWorkarea(Guid workareaId, CancellationToken ct = default)
		{
			return await _context.WorkareaCheckinPoints
				.Where(x => x.WorkareaId == workareaId && !x.IsDeleted)
				.CountAsync(ct);
		}

		public async Task<bool> ExistsByWorkareaAndCode(
		Guid workareaId,
		string code,
		CancellationToken ct = default)
		{
			var normalizedCode = code.Trim().ToUpper();

			return await _context.WorkareaCheckinPoints.AnyAsync(x => x.WorkareaId == workareaId
							&& x.Code == normalizedCode, ct);
		}

		public async Task<WorkareaCheckinPoint?> GetFirstByWorkarea(Guid workareaId, CancellationToken ct)
		{
			return await _context.WorkareaCheckinPoints
				.Where(x => x.WorkareaId == workareaId)
				.OrderBy(x => x.Created)
				.FirstOrDefaultAsync(ct);
		}

		public async Task<PaginatedResult<WorkareaCheckinPoint>> GetsPaging(GetsCheckinPointQuery query, PaginationRequest request, CancellationToken ct = default)
		{
			var checkinPoints = _context.WorkareaCheckinPoints.AsQueryable();

			if (!string.IsNullOrWhiteSpace(query.Name))
			{
				checkinPoints = checkinPoints.Where(x =>
					EF.Functions.ILike(x.Name, $"%{query.Name}%")
				);
			}

			if (!string.IsNullOrWhiteSpace(query.Code))
			{
				checkinPoints = checkinPoints.Where(x =>
					EF.Functions.ILike(x.Code, $"%{query.Code}%")
				);
			}

			checkinPoints = checkinPoints.OrderByDescending(x => x.Id);

			return await checkinPoints.ToPaginatedResultAsync(request, ct);
		}
	}
}
