using CleanOpsAi.BuildingBlocks.Domain.Dtos; 

namespace CleanOpsAi.BuildingBlocks.Infrastructure.Events
{

	public class GenerateTaskAssignmentsRequestedEvent
	{
		public List<GenerateTaskAssignmentItem> Items { get; set; } = new();
	}
	public record GenerateTaskAssignmentItem
	{
		public Guid ScheduleId { get; init; }
		public Guid? AssigneeId { get; init; }
		public Guid WorkAreaId { get; init; }
		public DateOnly FromDate { get; init; }
		public DateOnly ToDate { get; init; }
		public RecurrenceType RecurrenceType { get; init; }
		public RecurrenceConfig RecurrenceConfig { get; init; }
		public int DurationMinutes { get; init; }
		public string AssigneeName { get; init; } = default!;
		public string DisplayLocation { get; init; } = default!;
		public string TaskName { get; init; } = default!;
		public string Source { get; init; } = default!;
	}
}
