using CleanOpsAi.Modules.Workforce.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanOpsAi.Modules.Workforce.Domain.Entities
{
	[Table("worker_skills")]
	public class WorkerSkill
	{
		public Guid WorkerId { get; set; }

		public Guid SkillId { get; set; }

		public SkillLevelType SkillLevel { get; set; }

		public virtual Worker Worker { get; set; } = null!;

		public virtual Skill Skill { get; set; } = null!;
	}
}
