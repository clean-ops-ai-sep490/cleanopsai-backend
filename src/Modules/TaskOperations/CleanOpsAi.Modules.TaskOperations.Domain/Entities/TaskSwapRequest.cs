using CleanOpsAi.BuildingBlocks.Domain;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanOpsAi.Modules.TaskOperations.Domain.Entities
{
	[Table("task_swap_requests")]
	public class TaskSwapRequest : BaseAuditableEntity
	{
		public Guid TaskAssignmentId { get; set; }

		public Guid RequesterId { get; set; }

		public Guid? TargetWorkerId { get; set; }

		public SwapRequestStatus Status { get; set; }

		public Guid? ReviewedByUserId { get; set; }	 

		public TaskAssignment TaskAssignment { get; set; } = null!;
	}
}
