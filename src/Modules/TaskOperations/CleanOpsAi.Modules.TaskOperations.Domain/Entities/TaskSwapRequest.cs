using CleanOpsAi.BuildingBlocks.Domain;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanOpsAi.Modules.TaskOperations.Domain.Entities
{
		[Table("task_swap_requests")]
		public class TaskSwapRequest : BaseAuditableEntity
		{
			public Guid TaskAssignmentId { get; set; }
			public Guid TargetTaskAssignmentId { get; set; }

			public Guid RequesterId { get; set; }
			public Guid TargetWorkerId { get; set; }

			public SwapRequestStatus Status { get; set; }

			public string? RequesterNote { get; set; } 

			public Guid? ReviewedByUserId { get; set; } 
			public string? ReviewNote { get; set; }
			public DateTime ExpiredAt { get; set; }

			public string RequesterName { get; set; } = null!;
			public string TargetWorkerName { get; set; } = null!;
			public string? ReviewerName { get; set; }

			public TaskAssignment TaskAssignment { get; set; } = null!;
			public TaskAssignment TargetTaskAssignment { get; set; } = null!;
		}
}
