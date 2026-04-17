using CleanOpsAi.BuildingBlocks.Domain.Dtos.Notifications;

namespace CleanOpsAi.Modules.QualityControl.Application.DTOs.Request
{
	public class NotificationRecipientCreateDto
	{
		public Guid NotificationId { get; set; }

		public RecipientTypeEnum RecipientType { get; set; }

		public Guid? RecipientId { get; set; }
	}
}
