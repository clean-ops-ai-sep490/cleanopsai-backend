using System.ComponentModel.DataAnnotations.Schema;

namespace CleanOpsAi.Modules.ServicePlanning.Domain.Entities
{
	[Table("sop_required_certifications")]
	public class SopRequiredCertification
	{
		public Guid SopId { get; set; }

		public Guid CertificationId { get; set; }

		public virtual Sop Sop { get; set; } = null!;
	}
}
