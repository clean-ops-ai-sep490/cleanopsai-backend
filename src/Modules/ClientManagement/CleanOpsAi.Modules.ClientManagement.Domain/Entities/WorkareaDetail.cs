using CleanOpsAi.BuildingBlocks.Domain; 
using System.ComponentModel.DataAnnotations.Schema; 

namespace CleanOpsAi.Modules.ClientManagement.Domain.Entities
{
	[Table("work_area_details")]
	public class WorkAreaDetail : BaseAuditableEntity
	{
		public string Name { get; set; } = null!; 

		public double Area { get; set; }

		public double TotalArea { get; set; }

		public Guid WorkAreaId { get; set; }

		public virtual WorkArea WorkArea { get; set; } = null!;

	}
}
