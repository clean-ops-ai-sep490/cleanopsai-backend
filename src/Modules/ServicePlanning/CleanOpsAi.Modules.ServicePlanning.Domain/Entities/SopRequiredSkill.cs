using System.ComponentModel.DataAnnotations.Schema;

namespace CleanOpsAi.Modules.ServicePlanning.Domain.Entities
{
	[Table("sop_required_skills")]
	public class SopRequiredSkill
	{
		public Guid SopId { get; set; }

		public Guid SkillId { get; set; }

		public virtual Sop Sop { get; set; } = null!;
	}
}
