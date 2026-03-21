using CleanOpsAi.BuildingBlocks.Application.Pagination;

namespace CleanOpsAi.Modules.ServicePlanning.Application.Common.Interfaces.Services
{
	public interface ISopService 
	{
		Task<SopDto?> GetSopByIdAsync(Guid id);

		Task<SopDto> CreateSopAsync(SopCreateDto dto);

		Task<SopDto?> UpdateSopAsync(Guid id, SopUpdateDto dto, CancellationToken cancellationToken = default);

		Task<bool> DeleteSopAsync(Guid id);

		Task<SopDto?> GetByIdWithStepsAsync(Guid id, CancellationToken cancellationToken = default);

		Task<PaginatedResult<SopDto>> Gets(PaginationRequest request, CancellationToken ct = default);
	}
}
