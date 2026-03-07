using CleanOpsAi.BuildingBlocks.Domain;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanOpsAi.Modules.ClientManagement.Domain.Entities
{
	[Table("service_level_agreements")]
	public class ServiceLevelAgreement : BaseAuditableEntity
	{
		public string Name { get; set; } = null!;

		public string? Description { get; set; } 

		public EnvironmentType EnvironmentType { get; set; }

		public ServiceType ServiceType { get; set; }

		public Guid WorkAreaId { get; set; }
		public WorkArea WorkArea { get; set; } = null!;

		public Guid ContractId { get; set; }
		public Contract Contract { get; set; } = null!;
	}
}
