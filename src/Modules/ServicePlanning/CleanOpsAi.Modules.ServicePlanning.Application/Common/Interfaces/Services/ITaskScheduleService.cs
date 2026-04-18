using CleanOpsAi.BuildingBlocks.Application.Common;
using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.BuildingBlocks.Domain.Dtos.Sops;
using CleanOpsAi.Modules.ServicePlanning.Application.DTOs;

namespace CleanOpsAi.Modules.ServicePlanning.Application.Common.Interfaces.Services
{
	public interface ITaskScheduleService
	{
		Task<TaskScheduleDto> GetById(Guid id, CancellationToken ct = default);

		Task<TaskScheduleDto> Create(TaskScheduleCreateDto createDto, CancellationToken ct = default);

		Task<TaskScheduleDto> Update(Guid id, TaskScheduleUpdateDto dto, CancellationToken ct = default);

		Task<bool> Delete(Guid id, CancellationToken ct = default);

		Task<IReadOnlyList<ActiveTaskScheduleDto>> GetActiveSchedulesAsync();
		 
		Task GenerateTaskAssigmentsAsync(GenerateTaskAssignmentsRequest request, CancellationToken ct = default);

		Task<PaginatedResult<TaskScheduleDto>> Gets(GetsTaskScheduleQuery query, PaginationRequest request, CancellationToken ct = default);

		Task<Result> Activate(Guid id, CancellationToken ct = default);
		Task<Result> Deactivate(Guid id, CancellationToken ct = default);

		Task<List<SopStepMetadataDto>> GetSopStepsWithSchemaAsync(
			Guid sopId,
			CancellationToken ct = default);
	}
}
