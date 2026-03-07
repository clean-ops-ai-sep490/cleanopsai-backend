using CleanOpsAi.BuildingBlocks.Domain;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanOpsAi.Modules.ClientManagement.Domain.Entities
{
	[Table("contracts")]
	public class Contract : BaseAuditableEntity
	{
		public string Name { get; set; } = null!;

		public string UrlFile { get; set; } = null!;

		public Guid ClientId { get; set; }

		public Client Client { get; set; } = null!;

		public virtual ICollection<ContractShift> ContractShifts { get; set; } = new List<ContractShift>();

		public virtual ICollection<ServiceLevelAgreement> ServiceLevelAgreements { get; set; } = new List<ServiceLevelAgreement>();
	}
}
