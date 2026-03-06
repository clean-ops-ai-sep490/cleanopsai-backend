using CleanOpsAi.BuildingBlocks.Domain; 
using System.ComponentModel.DataAnnotations.Schema; 

namespace CleanOpsAi.Modules.ClientManagement.Domain.Entities
{
	[Table("workarea_details")]
	public class WorkareaDetail : BaseAuditableEntity
	{
		public string Name { get; set; } = null!;


	}
}
