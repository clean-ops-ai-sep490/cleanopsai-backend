using CleanOpsAi.BuildingBlocks.Application.Pagination;

namespace CleanOpsAi.Modules.ServicePlanning.Application.Common.Interfaces.Services
{
	public interface IStepService
	{
		Task<StepDto> GetStepById(Guid id, CancellationToken ct = default);

		Task<StepDto> CreateNewStep(StepCreateDto dto, Guid userId, CancellationToken ct = default);

		Task<StepDto> UpdateStep(Guid id, StepUpdateDto dto, Guid userId, CancellationToken ct = default);

		Task<bool> DeleteStep(Guid id, CancellationToken ct = default);

		Task<PaginatedResult<StepDto>> Gets(PaginationRequest request, CancellationToken ct = default);

	}
}
