using CleanOpsAi.BuildingBlocks.Application.Pagination;

namespace CleanOpsAi.Modules.ServicePlanning.Application.Common.Interfaces.Services
{
	public interface IStepService
	{
		Task<StepDto> GetStepById(Guid id);

		Task<StepDto> CreateNewStep(StepCreateDto dto);

		Task<StepDto> UpdateStep(Guid id, StepUpdateDto dto);

		Task<bool> DeleteStep(Guid id);

		Task<PaginatedResult<StepDto>> Gets(PaginationRequest request, CancellationToken ct = default);

	}
}
