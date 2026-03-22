namespace CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request
{
	public class RespondSwapRequestDto
	{
		public Guid SwapRequestId { get; set; }
		public bool IsAccepted { get; set; }
	}
}
