using CleanOpsAi.BuildingBlocks.Domain;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanOpsAi.Modules.TaskOperations.Domain.Entities
{
	[Table("emergency_leave_requests")]
	public class EmergencyLeaveRequest : BaseAuditableEntity
	{ 
		public Guid WorkerId { get; set; }

		public Guid TaskAssignmentId { get; set; } 

		public string? AudioUrl { get; set; }

		public string? Transcription { get; set; }

		public RequestStatus Status { get; set; }

		public string? ReviewedById { get; set; }  

		public DateTime? ApprovedAt { get; set; }

		public virtual TaskAssignment TaskAssignment { get; set; } = null!;
	}
}
