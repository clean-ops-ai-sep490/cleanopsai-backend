using CleanOpsAi.BuildingBlocks.Infrastructure.Events;

namespace CleanOpsAi.Modules.TaskOperations.Application.DTOs
{
	public class PpeCheckNotification
	{
		public Guid TaskStepExecutionId { get; set; }
		public string Status { get; set; } = string.Empty;
		public string? Message { get; set; }
		//public List<PpeDetectedItem> DetectedItems { get; set; } = [];
		public List<string> MissingItems { get; set; } = [];
		public DateTime At { get; set; }
	}
}
