namespace CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request
{
	public class TaskAssignmentUpdateDto
	{
		public Guid TaskScheduleId { get; set; }

		public Guid AssigneeId { get; set; }

		public DateTime ScheduledStartAt { get; set; }

		public string? DisplayLocation { get; set; }
	}
}
