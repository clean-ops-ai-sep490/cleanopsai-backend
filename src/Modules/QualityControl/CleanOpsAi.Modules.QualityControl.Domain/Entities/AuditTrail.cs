namespace CleanOpsAi.Modules.QualityControl.Domain.Entities
{
	public class AuditTrail
	{
		public Guid Id { get; set; }

		public string TraceId { get; set; } = string.Empty;

		public string EntityName { get; set; } = null!;
		public string EntityId { get; set; } = null!;

		public string Action { get; set; } = null!;

		public string UserId { get; set; } = null!;
		public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

		public string? OldValues { get; set; }
		public string? NewValues { get; set; }

		public string? IpAddress { get; set; }
		public string? Notes { get; set; }

		public string Source { get; set; } = "API";
	}
}

