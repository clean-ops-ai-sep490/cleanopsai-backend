using CleanOpsAi.BuildingBlocks.Domain;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanOpsAi.Modules.ClientManagement.Domain.Entities
{
	[Table("clients")]
	public class Client : BaseAuditableEntity
	{
		public string Name { get; set; } = null!;
		public string Email { get; set; } = null!;
	}
}
