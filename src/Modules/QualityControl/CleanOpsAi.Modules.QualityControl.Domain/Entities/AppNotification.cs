using CleanOpsAi.BuildingBlocks.Domain;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace CleanOpsAi.Modules.QualityControl.Domain.Entities
{
	[Table("notifications")]
	public class AppNotification : BaseAuditableEntity
	{
		public string Title { get; set; } = null!;

		public string Body { get; set; } = null!;

		[Column(TypeName = "jsonb")]
		public string Payload { get; set; } = "{}";

		public int Priority { get; set; }

		public string SenderType { get; set; } = null!;

		public string SenderId { get; set; } = null!;
	}
}
