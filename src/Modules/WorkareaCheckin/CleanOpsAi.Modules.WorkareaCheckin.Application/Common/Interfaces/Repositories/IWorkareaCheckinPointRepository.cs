using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.Modules.WorkareaCheckin.Application.DTOs.Request;
using CleanOpsAi.Modules.WorkareaCheckin.Domain.Entities;

namespace CleanOpsAi.Modules.WorkareaCheckin.Application.Common.Interfaces.Repositories
{
	public interface IWorkareaCheckinPointRepository : IBaseRepo<WorkareaCheckinPoint, Guid>
	{
		Task<bool> ExistsByWorkareaAndCode(Guid workareaId, string code, CancellationToken ct = default);

		Task<int> CountByWorkarea(Guid workareaId, CancellationToken ct = default);

		Task<WorkareaCheckinPoint?> GetFirstByWorkarea(Guid workareaId, CancellationToken ct);

		Task<PaginatedResult<WorkareaCheckinPoint>> GetsPaging(GetsCheckinPointQuery query, PaginationRequest request, CancellationToken ct = default);
	}
}
