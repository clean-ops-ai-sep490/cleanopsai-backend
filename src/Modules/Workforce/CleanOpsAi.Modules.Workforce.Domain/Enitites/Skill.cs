using CleanOpsAi.BuildingBlocks.Domain;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanOpsAi.Modules.Workforce.Domain.Enitites
{
	[Table("skill")]
	public class Skill : BaseAuditableEntity
	{
		public string Name { get; set; } = null!;

		public string? Description { get; set; }

		public virtual ICollection<WorkerSkill> WorkerSkills { get; set; } = new List<WorkerSkill>();
	}
}
