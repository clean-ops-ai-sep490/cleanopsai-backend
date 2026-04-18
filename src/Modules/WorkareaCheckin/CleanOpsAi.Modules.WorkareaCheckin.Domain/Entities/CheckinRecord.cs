using CleanOpsAi.BuildingBlocks.Domain;
using CleanOpsAi.Modules.WorkareaCheckin.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanOpsAi.Modules.WorkareaCheckin.Domain.Entities
{
	[Table("checkin_records")]
	public class CheckinRecord : BaseAuditableEntity
	{
		public Guid WorkerId { get; set; } 

		public Guid WorkareaCheckinPointId { get; set; }

		public Guid? AccessDeviceId { get; set; }

		public Guid? TaskId { get; set; }
		public Guid? TaskStepId { get; set; }

		public DateTime CheckinAt { get; set; }

		public CheckinType CheckinType { get; set; } = CheckinType.Manual;
		public CheckinStatus Status { get; set; } = CheckinStatus.Active;

		public string? Notes { get; set; }
		public string? Metadata { get; set; }

		public WorkareaCheckinPoint WorkareaCheckinPoint { get; set; } = null!;

		public AccessDevice? AccessDevice { get; set; }
	}
}
