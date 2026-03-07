using CleanOpsAi.BuildingBlocks.Domain;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanOpsAi.Modules.TaskOperations.Domain.Entities
{
	[Table("task_assignments")]
	public class TaskAssignment : BaseAuditableEntity
	{
		 public Guid TaskScheduleId { get; set; }

		public Guid AssigneeId { get; set; }

		public Guid OriginalAssigneeId { get; set; }

		public TaskAssignmentStatus Status { get; set; }  

		public DateTime ScheduledStartAt { get; set; }

		public bool IsAdhocTask { get; set; }

		public string? NameAdhocTask { get; set; }

		public string? DisplayLocation { get; set; }

		public virtual ICollection<TaskStepExecution> TaskStepExecutions { get; set; } = new List<TaskStepExecution>();

		public virtual ICollection<TaskSwapRequest> TaskSwapRequests { get; set; } = new List<TaskSwapRequest>();
	}
}
