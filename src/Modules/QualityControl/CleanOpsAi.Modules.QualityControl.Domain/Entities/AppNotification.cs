using CleanOpsAi.BuildingBlocks.Domain;
using CleanOpsAi.BuildingBlocks.Domain.Dtos.Notifications; 
using System.ComponentModel.DataAnnotations.Schema; 

namespace CleanOpsAi.Modules.QualityControl.Domain.Entities
{
	[Table("notifications")]
	public class AppNotification : BaseAuditableEntity
	{
		public string Title { get; set; } = null!;

		public string Body { get; set; } = null!;

		[Column(TypeName = "jsonb")]
		public string Payload { get; set; } = "{}";

		public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

		public SenderTypeEnum SenderType { get; set; } = SenderTypeEnum.System;

		public Guid? SenderId { get; set; } 

		public virtual ICollection<NotificationRecipient> NotificationRecipients { get; set; } = new List<NotificationRecipient>();
	}
}
