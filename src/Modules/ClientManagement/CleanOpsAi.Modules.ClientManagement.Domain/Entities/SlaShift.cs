using CleanOpsAi.BuildingBlocks.Domain;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanOpsAi.Modules.ClientManagement.Domain.Entities
{
	[Table("sla_shifts")]
	public class SlaShift : BaseAuditableEntity
	{
		public string Name { get; set; } = null!;

		public Guid SlaId { get; set; }

		public TimeOnly StartTime { get; set; }

		public TimeOnly EndTime { get; set; }

		public int RequiredWorker { get; set; }

		public int BreakTime { get; set; } 

		public Sla Sla { get; set; } = null!;
	}
}
