using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.Modules.ServicePlanning.Domain.Entities;

namespace CleanOpsAi.Modules.ServicePlanning.Application.Common.Interfaces.Services
{
	public interface ISopService 
	{
		Task<SopDto?> GetSopByIdAsync(Guid id, CancellationToken ct = default);

		Task<SopDto> CreateSopAsync(SopCreateDto dto, CancellationToken ct = default);

		Task<SopDto?> UpdateSopAsync(Guid id, SopUpdateDto dto, CancellationToken cancellationToken = default);

		Task<bool> DeleteSopAsync(Guid id, CancellationToken ct = default);

		Task<SopDto?> GetByIdWithStepsAsync(Guid id, CancellationToken cancellationToken = default);

		Task<PaginatedResult<SopDto>> Gets(PaginationRequest request, CancellationToken ct = default);

		Task<Sop?> GetSopWithDetail(Guid id, CancellationToken ct = default);
	}
}
