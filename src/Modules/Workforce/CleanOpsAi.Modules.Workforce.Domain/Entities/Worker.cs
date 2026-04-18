using System.ComponentModel.DataAnnotations.Schema;

namespace CleanOpsAi.Modules.Workforce.Domain.Entities
{
	[Table("workers")]
	public class Worker : BaseAuditableEntity
	{
		public Guid UserId { get; set; }

		public string FullName { get; set; } = null!;

        public string? DisplayAddress { get; set; }

		public double? Latitude { get; set; }

		public double? Longitude { get; set; }

		public string AvatarUrl { get; set; } = null!;

		public virtual ICollection<WorkerCertification> WorkerCertifications { get; set; } = new List<WorkerCertification>();

		public virtual ICollection<WorkerSkill> WorkerSkills { get; set; } = new List<WorkerSkill>();

		public virtual ICollection<WorkerGps> WorkerGps { get; set; } = new List<WorkerGps>();
	}
}
