using CleanOpsAi.BuildingBlocks.Domain;
using CleanOpsAi.Modules.WorkareaCheckin.Domain.Enums;
using CleanOpsAi.Modules.WorkareaCheckin.Domain.Owned;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanOpsAi.Modules.WorkareaCheckin.Domain.Entities
{
	[Table("access_devices")]
	public class AccessDevice : BaseAuditableEntity
	{
		public Guid WorkareaCheckinPointId { get; set; }

		public DeviceType DeviceType { get; set; } = DeviceType.QrStatic;

		[MaxLength(100)]
		public string? Name { get; set; }

		[MaxLength(50)]
		public string? Code { get; set; }

		[MaxLength(200)]
		public string? Identifier { get; set; }

		public DeviceStatus Status { get; set; } = DeviceStatus.Active; 

		public DevicePosition? Position { get; set; }
		public BleBeaconInfo? BleInfo { get; set; }

		public WorkareaCheckinPoint WorkareaCheckinPoint { get; set; } = null!;
	}
}
