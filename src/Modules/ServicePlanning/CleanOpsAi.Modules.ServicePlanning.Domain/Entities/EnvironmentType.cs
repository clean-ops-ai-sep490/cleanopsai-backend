using CleanOpsAi.BuildingBlocks.Domain;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanOpsAi.Modules.ServicePlanning.Domain.Entities
{
	[Table("environment_types")]
	public class EnvironmentType : BaseAuditableEntity
	{
		public string Name { get; set; } = null!;

		public string? Description { get; set; }

		public ICollection<Sop> Sops { get; set; } = new List<Sop>();
	}
}
