using CleanOpsAi.Modules.TaskOperations.Domain.Enums; 

namespace CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request
{
	public enum SwapPerspective { All, Sent, Received }

	public class GetMySwapRequestsDto
	{
		public Guid WorkerId { get; set; }
		public SwapPerspective Perspective { get; set; } = SwapPerspective.All;
		public SwapRequestStatus? Status { get; set; }
	}

}
