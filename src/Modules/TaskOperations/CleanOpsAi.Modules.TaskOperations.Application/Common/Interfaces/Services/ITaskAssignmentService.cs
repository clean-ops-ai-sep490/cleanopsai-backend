using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.BuildingBlocks.Infrastructure.Events;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Response;
using CleanOpsAi.Modules.TaskOperations.Domain.Enums;

namespace CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services
{
	public interface ITaskAssignmentService
	{
		Task<TaskAssignmentDto?> GetById(Guid id, CancellationToken ct = default);

		Task<TaskAssignmentDto?> Update(Guid id, TaskAssignmentDto dto);

		Task<bool> Delete(Guid id);

		Task<bool> UpdateStatus(Guid id, TaskAssignmentStatus status); 

		Task GenerateAsync(GenerateTaskAssignmentsRequestedEvent msg);

		Task<StartTaskDto> StartTaskAsync(
			Guid taskAssignmentId,
			Guid workerId,
			CancellationToken ct = default);

		Task<PaginatedResult<TaskAssignmentDto>> Gets(TaskAssignmentFilter filter, PaginationRequest request,
			CancellationToken ct = default);


		Task<StartTaskDto> CompleteTaskAsync(Guid taskAssignmentId, TaskCompletedDto dto, CancellationToken ct = default);
	}
}
