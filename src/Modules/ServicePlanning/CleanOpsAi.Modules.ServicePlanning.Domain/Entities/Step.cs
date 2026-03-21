using CleanOpsAi.BuildingBlocks.Domain;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanOpsAi.Modules.ServicePlanning.Domain.Entities
{
	[Table("steps")] 
	public class Step : BaseAuditableEntity
	{
		public string ActionKey { get; set; } = null!;

		public string Name { get; set; } = null!;

		public string? Description { get; set; }

		public string ConfigSchema { get; set; } = null!;

		public bool IsActive { get; set; }

		public virtual ICollection<SopStep> SopSteps { get; set; } = new List<SopStep>();
	}
}
