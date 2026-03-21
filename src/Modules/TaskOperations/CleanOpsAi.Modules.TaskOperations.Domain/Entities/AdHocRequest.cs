
using CleanOpsAi.BuildingBlocks.Domain;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanOpsAi.Modules.TaskOperations.Domain.Entities
{
	[Table("adhoc_requests")]
	public class AdHocRequest : BaseAuditableEntity
	{ 
		public Guid TaskAssignmentId { get; set; }

		public Guid RequestedByWorkerId { get; set; }   

		public AdHocRequestType RequestType { get; set; }

		public string? Reason { get; set; }

		public string? Description { get; set; }

		public AdHocRequestStatus Status { get; set; }

		public Guid? ReviewedByUserId { get; set; }  

		public DateTime? ApprovedAt { get; set; }

		public virtual TaskAssignment TaskAssignment { get; set; } = null!;
	}
}
