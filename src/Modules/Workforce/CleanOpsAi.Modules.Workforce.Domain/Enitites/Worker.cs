using CleanOpsAi.BuildingBlocks.Domain;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanOpsAi.Modules.Workforce.Domain.Enitites
{
	[Table("workers")]
	public class Worker : BaseAuditableEntity
	{
		public string UserId { get; set; } = null!;

		public string? DisplayAddress { get; set; }

		public double? Latitude { get; set; }

		public double? Longitude { get; set; }

		public string AvatarUrl { get; set; } = null!;

		public virtual ICollection<WorkerCertification> WorkerCertifications { get; set; } = new List<WorkerCertification>();

		public virtual ICollection<WorkerSkill> WorkerSkills { get; set; } = new List<WorkerSkill>();
	}
}
