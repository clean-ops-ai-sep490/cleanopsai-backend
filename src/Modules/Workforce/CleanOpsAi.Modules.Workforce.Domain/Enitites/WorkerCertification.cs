using System.ComponentModel.DataAnnotations.Schema;

namespace CleanOpsAi.Modules.Workforce.Domain.Enitites
{
	[Table("worker_certifications")]
	public class WorkerCertification
	{
		public Guid WorkerId { get; set; } 

		public Guid CertificationId { get; set; }

		public DateTime IssuedDate { get; set; }

		public DateTime? ExpiredAt { get; set; }

		public virtual Worker Worker { get; set; } = null!;

		public virtual Certification Certification { get; set; } = null!;

	}
}
