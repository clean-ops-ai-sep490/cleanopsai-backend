using CleanOpsAi.Modules.TaskOperations.Domain.Enums;

namespace CleanOpsAi.Modules.TaskOperations.Application.DTOs.Response
{
	public class TaskSwapRequestDto
	{
		public Guid Id { get; set; }

		public Guid TaskAssignmentId { get; set; }

		public Guid RequesterId { get; set; }

		public Guid? TargetWorkerId { get; set; }

		public SwapRequestStatus Status { get; set; }

		public Guid? ReviewedByUserId { get; set; }
	}
}
