using CleanOpsAi.BuildingBlocks.Domain;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanOpsAi.Modules.TaskOperations.Domain.Entities
{
	[Table("issue_reports")]
	public class IssueReport : BaseAuditableEntity
	{
		public Guid TaskAssignmentId { get; set; }

		public Guid ReportedByWorkerId { get; set; } 

		public string Description { get; set; } = null!;

		public IssueStatus Status { get; set; }

		public string? ResolvedByUserId { get; set; }

		public DateTime? ResolvedAt { get; set; }

		public virtual TaskAssignment TaskAssignment { get; set; } = null!;
	
	}
}
