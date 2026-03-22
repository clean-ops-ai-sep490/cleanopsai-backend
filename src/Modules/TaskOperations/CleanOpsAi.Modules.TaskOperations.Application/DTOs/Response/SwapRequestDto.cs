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
		public SwapTaskInfoDto RequesterTask { get; set; } = null!;

		// Task of B
		public Guid TargetWorkerId { get; set; }
		public SwapTaskInfoDto TargetTask { get; set; } = null!;
	}
}
