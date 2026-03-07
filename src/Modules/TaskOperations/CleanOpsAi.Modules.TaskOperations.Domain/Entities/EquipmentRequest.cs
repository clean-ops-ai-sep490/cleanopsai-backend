using CleanOpsAi.BuildingBlocks.Domain;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanOpsAi.Modules.TaskOperations.Domain.Entities
{
	[Table("equipment_requests")]
	public class EquipmentRequest : BaseAuditableEntity
	{
		public Guid TaskAssignmentId { get; set; }

		public Guid WorkerId { get; set; }

		public Guid EquipmentId { get; set; }

		public string? ReviewedBy { get; set; }

		public int Quantity { get; set; }

		public string? Reason { get; set; }

		public EquipmentRequestStatus Status { get; set; }
	}
}
