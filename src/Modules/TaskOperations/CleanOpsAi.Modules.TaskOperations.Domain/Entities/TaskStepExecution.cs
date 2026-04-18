using CleanOpsAi.BuildingBlocks.Domain;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanOpsAi.Modules.TaskOperations.Domain.Entities
{
	[Table("task_step_executions")]
	public class TaskStepExecution : BaseAuditableEntity
	{
		public Guid TaskAssignmentId { get; set; }

		public Guid SopStepId { get; set; }

		public TaskStepExecutionStatus Status { get; set; }

		public string ConfigSnapshot { get; set; } = null!; // config detail + configschema jsonb

		public string ResultData { get; set; } = null!; //jsonb

		public int StepOrder { get; set; }

		public DateTime StartedAt { get; set; }

		public DateTime CompletedAt { get; set; }

		public virtual TaskAssignment TaskAssignment { get; set; } = null!;

		public virtual ICollection<ComplianceCheck> ComplianceChecks { get; set; } = new List<ComplianceCheck>();

		public virtual ICollection<TaskStepExecutionImage> TaskStepExecutionImages { get; set; } = new List<TaskStepExecutionImage>();
	}
}
