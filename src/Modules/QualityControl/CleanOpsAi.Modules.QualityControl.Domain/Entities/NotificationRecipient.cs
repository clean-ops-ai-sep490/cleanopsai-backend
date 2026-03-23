using CleanOpsAi.BuildingBlocks.Domain;
using CleanOpsAi.BuildingBlocks.Domain.Dtos.Notifications;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanOpsAi.Modules.QualityControl.Domain.Entities
{
	[Table("notification_recipients")]
	public class NotificationRecipient : BaseAuditableEntity
	{
		public Guid NotificationId { get; set; }

		public RecipientTypeEnum RecipientType { get; set; }

		public Guid RecipientId { get; set; }

		public bool IsRead { get; set; }  

		public DateTime? IsReadAt { get; set; }

		public AppNotification AppNotification { get; set; } = null!;
	}
}
