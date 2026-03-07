using CleanOpsAi.BuildingBlocks.Domain;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanOpsAi.Modules.Workforce.Domain.Entities
{
	[Table("certifications")]
	public class Certification : BaseAuditableEntity
	{
		public string Name { get; set; } = null!;

		public string IssuingOrganization { get; set; } = null!; 

		public virtual ICollection<WorkerCertification> WorkerCertifications { get; set; } = new List<WorkerCertification>();
	}
}
