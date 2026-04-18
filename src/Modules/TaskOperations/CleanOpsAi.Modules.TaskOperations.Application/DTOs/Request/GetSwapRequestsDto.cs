using CleanOpsAi.Modules.TaskOperations.Domain.Enums; 

namespace CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request
{
	public class GetSwapRequestsDto
	{
		public SwapRequestStatus? Status { get; set; }
	}
}
