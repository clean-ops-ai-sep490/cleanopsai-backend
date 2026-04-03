using CleanOpsAi.Modules.TaskOperations.Application.DTOs;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request;

namespace CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services
{
	public interface ITaskStepExecutionService
	{
		Task<TaskStepExecutionDto> CompleteStepAsync(
			Guid id,
			SubmitStepExecutionDto dto, 
			CancellationToken ct = default);
	}
}
