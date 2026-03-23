namespace CleanOpsAi.Modules.TaskOperations.Application.DTOs.Response
{
	public class SwapTaskInfoDto
	{
		public Guid TaskAssignmentId { get; set; }
		public DateTime ScheduledStartAt { get; set; }
		public DateTime ScheduledEndAt { get; set; }
		public string? DisplayLocation { get; set; }
	}
}
