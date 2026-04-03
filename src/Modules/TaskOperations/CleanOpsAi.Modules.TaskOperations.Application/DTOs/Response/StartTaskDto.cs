using CleanOpsAi.Modules.TaskOperations.Domain.Enums; 

namespace CleanOpsAi.Modules.TaskOperations.Application.DTOs.Response
{
	public class StartTaskDto
	{
		public Guid TaskAssignmentId { get; init; }
		public TaskAssignmentStatus Status { get; init; }
		public List<TaskStepExecutionDto> Steps { get; init; } = new();
		public bool IsAdhoc { get; set; }
	}
}
