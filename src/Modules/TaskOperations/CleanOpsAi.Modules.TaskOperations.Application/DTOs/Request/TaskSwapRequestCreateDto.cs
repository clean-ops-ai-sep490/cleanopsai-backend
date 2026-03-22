namespace CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request
{
	public class TaskSwapRequestCreateDto
	{
		public Guid TaskAssignmentId { get; set; }
		public Guid TargetTaskAssignmentId { get; set; }
		public Guid TargetWorkerId { get; set; }
		public string? RequesterNote { get; set; }
	}
}
