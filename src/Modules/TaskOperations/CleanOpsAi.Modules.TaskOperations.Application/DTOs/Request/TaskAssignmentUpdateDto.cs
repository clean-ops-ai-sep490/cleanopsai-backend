namespace CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request
{
	public class TaskAssignmentUpdateDto
	{
		public string? TaskName { get; set; }

		public DateTime? ScheduledStartAt { get; set; }

		public int? DurationMinutes { get; set; }

		public Guid? AssigneeId { get; set; }

		public string? AssigneeName { get; set; } 

		public string? DisplayLocation { get; set; } 
	}
}
