using CleanOpsAi.BuildingBlocks.Domain;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanOpsAi.Modules.TaskOperations.Domain.Entities
{
	[Table("compliance_checks")]
	public class ComplianceCheck : BaseAuditableEntity
	{
		public Guid TaskStepExecutionId { get; set; }

		public ComplianceCheckStatus Status { get; set; }

		public ComplianceCheckType Type { get; set; }

		public Guid? SupervisorId { get; set; }
		public double MinScore { get; set; }        
		public int FailedImageCount { get; set; }

		public string? AIResultRaw { get; set; }

		public string? Feedback { get; set; }

		public TaskStepExecution TaskStepExecution { get; set; } = null!;
	}
}
