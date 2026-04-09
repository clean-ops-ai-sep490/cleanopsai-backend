using CleanOpsAi.BuildingBlocks.Domain;
using System.ComponentModel.DataAnnotations;

namespace CleanOpsAi.Modules.WorkareaCheckin.Domain.Entities
{
	public class WorkareaCheckinPoint : BaseAuditableEntity
	{
		public Guid WorkareaId { get; set; }

		[MaxLength(100)]
		public string Name { get; set; } = string.Empty;

		[MaxLength(50)]
		public string Code { get; set; } = string.Empty;

		public bool IsActive { get; set; } = true;

		public ICollection<AccessDevice> AccessDevices { get; set; } = new List<AccessDevice>();
	}
}
