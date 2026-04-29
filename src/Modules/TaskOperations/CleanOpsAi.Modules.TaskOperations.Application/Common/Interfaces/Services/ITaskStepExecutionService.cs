using CleanOpsAi.BuildingBlocks.Infrastructure.Events;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Response;

namespace CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services
{
	public interface ITaskStepExecutionService
	{
		Task<TaskStepExecutionDto> CompleteStepAsync(
			Guid id,
			SubmitStepExecutionDto dto, 
			CancellationToken ct = default); 

		Task<TaskStepExecutionDetailDto> GetStepDetailAsync(Guid id, CancellationToken ct = default);

		Task<TaskStepExecutionPpeCheckResponse> RequestPpeCheckAsync(Guid id, CancellationToken ct = default);

		Task ApplyPpeCheckResultAsync(PpeCheckCompletedEvent evt, CancellationToken ct = default);
	}
}
