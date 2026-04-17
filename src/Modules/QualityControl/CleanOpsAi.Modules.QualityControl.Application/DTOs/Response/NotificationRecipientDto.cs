using CleanOpsAi.BuildingBlocks.Domain.Dtos.Notifications;

namespace CleanOpsAi.Modules.QualityControl.Application.DTOs.Response
{
	public class NotificationRecipientDto
	{
		public Guid Id { get; set; }

		public Guid NotificationId { get; set; }

		public RecipientTypeEnum RecipientType { get; set; }

		public Guid? RecipientId { get; set; }

		public bool IsRead { get; set; }

		public DateTime? IsReadAt { get; set; }
	}
}
