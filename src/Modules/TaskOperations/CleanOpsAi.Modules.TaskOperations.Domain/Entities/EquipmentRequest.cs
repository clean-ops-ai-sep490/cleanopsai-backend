using CleanOpsAi.BuildingBlocks.Domain;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanOpsAi.Modules.TaskOperations.Domain.Entities
{
	[Table("equipment_requests")]
	public class EquipmentRequest : BaseAuditableEntity
	{
		public Guid TaskAssignmentId { get; set; }

		public Guid WorkerId { get; set; }

		public Guid? ReviewedByUserId { get; set; }

		public string? Reason { get; set; }

		public DateTime? ApprovedAt { get; set; }

		public EquipmentRequestStatus Status { get; set; }

        public ICollection<EquipmentRequestItem> Items { get; set; } = new List<EquipmentRequestItem>();
    }
}
