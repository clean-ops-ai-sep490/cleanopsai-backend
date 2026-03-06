using CleanOpsAi.BuildingBlocks.Domain;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanOpsAi.Modules.ClientManagement.Domain.Entities
{
	[Table("locations")]
	public class Location : BaseAuditableEntity
	{
		public string Name { get; set; } = null!;

		public string Address { get; set; } = null!;

		public string? Street { get; set; }

		public string? Commune { get; set; }

		public string? Province { get; set; }

		public double? Latitude { get; set; }

		public double? Longitude { get; set; }
	}
}
