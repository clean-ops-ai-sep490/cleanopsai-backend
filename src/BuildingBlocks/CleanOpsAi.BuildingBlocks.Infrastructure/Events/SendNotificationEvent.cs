
using CleanOpsAi.BuildingBlocks.Domain.Dtos.Notifications;

namespace CleanOpsAi.BuildingBlocks.Infrastructure.Events
{
	public record SendNotificationEvent
	{
		public string Title { get; init; } = null!;
		public string Body { get; init; } = null!;
		public string Payload { get; init; } = "{}";
		public NotificationPriority Priority { get; init; } = NotificationPriority.Normal;
		public SenderTypeEnum SenderType { get; init; } = SenderTypeEnum.System;
		public Guid? SenderId { get; init; }
		 
		public List<NotificationRecipientEvent> Recipients { get; init; } = new();
	}

	public record NotificationRecipientEvent
	{
		public RecipientTypeEnum RecipientType { get; init; }
		public Guid RecipientId { get; init; } // UserId / WorkerId / ...
	}
}
