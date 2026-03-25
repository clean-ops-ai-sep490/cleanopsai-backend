using CleanOpsAi.BuildingBlocks.Domain.Dtos.Notifications;
using System.Text.Json;

namespace CleanOpsAi.Modules.QualityControl.Application.DTOs.Response
{
	public class NotificationDto
	{
		public Guid Id { get; set; }

		public string Title { get; set; } = null!;

		public string Body { get; set; } = null!; 

		public JsonElement Payload { get; set; }  

		public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

		public SenderTypeEnum SenderType { get; set; } = SenderTypeEnum.System;

		public Guid? SenderId { get; set; }
	}
}
