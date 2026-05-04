namespace CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request
{
	public class SupervisorReviewRequest
	{
		public bool Approved { get; init; }
		public string? Feedback { get; init; }
	}
}
