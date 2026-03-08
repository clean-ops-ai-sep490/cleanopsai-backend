using System.ComponentModel.DataAnnotations.Schema;

namespace CleanOpsAi.Modules.Workforce.Domain.Entities
{
	[Table("equipments")]
	public class Equipment : BaseAuditableEntity
	{
		public string Name { get; set; } = null!; 

		public EquipmentType Type { get; set; }

		public string? Description { get; set; }
	}
}
