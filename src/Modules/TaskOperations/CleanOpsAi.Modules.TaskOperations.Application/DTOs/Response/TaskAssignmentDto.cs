using CleanOpsAi.Modules.TaskOperations.Domain.Enums;

namespace CleanOpsAi.Modules.TaskOperations.Application.DTOs.Response
{
	public class TaskAssignmentDto
	{
		public Guid Id { get; set; }

		public Guid TaskScheduleId { get; set; }

		public Guid AssigneeId { get; set; }

		public Guid OriginalAssigneeId { get; set; }

		public TaskAssignmentStatus Status { get; set; }

		public DateTime ScheduledStartAt { get; set; }

		public DateTime ScheduledEndAt { get; set; }

		public bool IsAdhocTask { get; set; }

		public string? NameAdhocTask { get; set; }

		public string? DisplayLocation { get; set; }

		public string AssigneeName { get; set; } = null!;

		public string OriginalAssigneeName { get; set; } = null!; 

		public ICollection<TaskStepExecutionDto> Steps { get; set; } = new List<TaskStepExecutionDto>();
	}
}
