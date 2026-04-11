using CleanOpsAi.Modules.WorkareaCheckin.Application.Common.Interfaces.Repositories;
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
	}
}
