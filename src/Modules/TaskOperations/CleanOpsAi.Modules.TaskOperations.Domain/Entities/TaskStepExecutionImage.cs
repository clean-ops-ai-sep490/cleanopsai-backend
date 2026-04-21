using CleanOpsAi.BuildingBlocks.Domain;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanOpsAi.Modules.TaskOperations.Domain.Entities
{
	[Table("task_step_execution_images")]
	public class TaskStepExecutionImage : BaseAuditableEntity
	{
		public Guid TaskStepExecutionId { get; set; }

		public string ImageUrl { get; set; } = null!;

		public ImageType ImageType { get; set; }

		public double? QualityScore { get; set; }
		public string? Verdict { get; set; }

		public virtual TaskStepExecution TaskStepExecution { get; set; } = null!;
	}
}
