
using CleanOpsAi.BuildingBlocks.Domain;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanOpsAi.Modules.ClientManagement.Domain.Entities
{
	[Table("sla_tasks")]
	public class SlaTask : BaseAuditableEntity
	{
		public string Name { get; set; } = null!;

		public Guid SlaId { get; set; }

		public string RecurrenceType { get; set; } = null!;

		public string RecurrenceConfig { get; set; } = null!; // JSON

		public Sla Sla { get; set; } = null!;
	}
}
