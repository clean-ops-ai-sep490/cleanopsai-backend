using System.Text.Json;

namespace CleanOpsAi.Modules.QualityControl.Application.DTOs.Response
{
	public class NotificationListItemDto
	{
		public Guid Id { get; set; }
		public Guid NotificationId { get; set; }
		public string Title { get; set; } = null!;
		public string Body { get; set; } = null!;
		public JsonElement Payload { get; set; } 
		public string Priority { get; set; } = null!;
		public string SenderType { get; set; } = null!;
		public Guid? SenderId { get; set; }
		public bool IsRead { get; set; }
		public DateTime? IsReadAt { get; set; }
		public DateTime Created { get; set; }
	}
}
