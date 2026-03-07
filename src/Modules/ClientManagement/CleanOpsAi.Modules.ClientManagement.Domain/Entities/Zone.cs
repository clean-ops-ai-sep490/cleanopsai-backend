using CleanOpsAi.BuildingBlocks.Domain;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanOpsAi.Modules.ClientManagement.Domain.Entities
{
	[Table("zones")]
	public class Zone : BaseAuditableEntity
	{
		public string Name { get; set; } = null!;

		public string? Description { get; set; }

		public Guid LocationId { get; set; }

		public Location Location { get; set; } = null!;

		public virtual ICollection<WorkArea> WorkAreas { get; set; } = new List<WorkArea>();
	}
}
