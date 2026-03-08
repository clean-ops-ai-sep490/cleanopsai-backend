using CleanOpsAi.BuildingBlocks.Domain;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanOpsAi.Modules.TaskOperations.Domain.Entities
{
	[Table("task_history")]
	public class TaskHistory : BaseAuditableEntity
	{
		public Guid TaskAssignmentId { get; set; }

		public Guid WorkerId { get; set; }

		public string Metadata { get; set; } = null!;

		public TaskAssignmentStatus Status { get; set; }

		public DateTime StartedAt { get; set; }

		public DateTime CompletedAt { get; set; }

		public virtual TaskAssignment TaskAssignment { get; set; } = null!;

	}
}
