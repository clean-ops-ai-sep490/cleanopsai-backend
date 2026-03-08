using CleanOpsAi.BuildingBlocks.Domain;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanOpsAi.Modules.ServicePlanning.Domain.Entities
{
	[Table("sop_steps")]	
	public class SopStep : BaseAuditableEntity
	{
		public Guid SopId { get; set; }

		public Guid StepId { get; set; }

		public int StepOrder { get; set; }

		public string ConfigDetail { get; set; } = null!;

		public virtual Sop Sop { get; set; } = null!;

		public virtual Step Step { get; set; } = null!;
	}
}
