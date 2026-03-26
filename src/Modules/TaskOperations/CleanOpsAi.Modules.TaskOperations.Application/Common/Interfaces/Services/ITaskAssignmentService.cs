using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.BuildingBlocks.Infrastructure.Events;
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

		Task<PaginatedResult<TaskAssignmentDto>> Gets(PaginationRequest request, CancellationToken ct = default);

		Task<PaginatedResult<TaskAssignmentDto>> GetsByAssigneeId(Guid assgineeId, PaginationRequest request, CancellationToken ct = default);

		Task GenerateAsync(GenerateTaskAssignmentsRequestedEvent msg);
	}
}
