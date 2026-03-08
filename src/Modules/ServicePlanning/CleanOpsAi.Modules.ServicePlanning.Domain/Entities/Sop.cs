using CleanOpsAi.BuildingBlocks.Domain;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanOpsAi.Modules.ServicePlanning.Domain.Entities
{
	[Table("sops")]
	public class Sop : BaseAuditableEntity
	{
		public string Name { get; set; } = null!;

		public string? Description { get; set; }

		public ServiceType ServiceType { get; set; }

		public EnvironmentType EnvironmentType { get; set; }

		public bool IsRequiredSkill { get; set; }

		public bool IsRequiredCertification { get; set; } 

		public int Version { get; set; }

		public virtual ICollection<SopStep> SopSteps { get; set; } = new List<SopStep>();
	}
}
