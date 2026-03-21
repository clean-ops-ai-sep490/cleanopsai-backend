using CleanOpsAi.BuildingBlocks.Application.Pagination;

namespace CleanOpsAi.Modules.ServicePlanning.Application.Common.Interfaces.Services
{
	public interface IEnvironmentTypeService
	{
		Task<EnvironmentTypeDto?> GetById(Guid id, CancellationToken ct = default);

		Task<EnvironmentTypeDto> Create(EnvironmentTypeCreateDto dto, CancellationToken ct = default);

		Task<EnvironmentTypeDto?> Update(Guid id, EnvironmentTypeUpdateDto dto, CancellationToken ct = default);

		Task<bool> Delete(Guid id, CancellationToken ct = default);

		Task<PaginatedResult<EnvironmentTypeDto>> Gets(PaginationRequest request, CancellationToken ct = default);
	}
}
