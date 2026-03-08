using CleanOpsAi.BuildingBlocks.Domain; 
using System.ComponentModel.DataAnnotations.Schema; 

namespace CleanOpsAi.Modules.ClientManagement.Domain.Entities
{
	[Table("work_areas")]
	public class WorkArea : BaseAuditableEntity
	{
		public string Name { get; set; } = null!; 

		public Guid ZoneId { get; set; }

		public virtual Zone Zone { get; set; } = null!;

		public virtual ICollection<Sla> Slas { get; set; } = new List<Sla>();

	}
}
