using CleanOpsAi.BuildingBlocks.Domain.Dtos.Notifications;
using System.Text.Json;

namespace CleanOpsAi.Modules.QualityControl.Application.DTOs.Request
{
	public class NotificationCreateDto
	{
		public string Title { get; set; } = null!;

		public string Body { get; set; } = null!;

		public JsonElement Payload { get; set; }

		public NotificationPriority Priority { get; set; } = NotificationPriority.Normal; 
	}
}
