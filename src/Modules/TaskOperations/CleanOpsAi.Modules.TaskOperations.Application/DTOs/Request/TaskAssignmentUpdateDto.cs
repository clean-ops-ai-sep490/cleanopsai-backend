namespace CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request
{
	public class TaskAssignmentUpdateDto
	{
		public DateTime ScheduledStartAt { get; set; }

		public DateTime ScheduledEndAt { get; set; }

		public string? DisplayLocation { get; set; }
	}
}
