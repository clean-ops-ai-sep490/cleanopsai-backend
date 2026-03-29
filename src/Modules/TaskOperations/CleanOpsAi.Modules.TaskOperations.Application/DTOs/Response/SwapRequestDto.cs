using CleanOpsAi.Modules.TaskOperations.Domain.Enums; 

namespace CleanOpsAi.Modules.TaskOperations.Application.DTOs.Response
{
	public class SwapRequestDto
	{
		public Guid Id { get; set; }
		public SwapRequestStatus Status { get; set; }
		public string? RequesterNote { get; set; }
		public string? ReviewNote { get; set; }
		public DateTime ExpiredAt { get; set; }
		public DateTime CreatedAt { get; set; }

		// Task of A
		public Guid RequesterId { get; set; } 

		// Task of B
		public Guid TargetWorkerId { get; set; }

		public string RequesterName { get; set; } = null!;
		public string TargetWorkerName { get; set; } = null!;
		public string? ReviewerName { get; set; }
	}
}
