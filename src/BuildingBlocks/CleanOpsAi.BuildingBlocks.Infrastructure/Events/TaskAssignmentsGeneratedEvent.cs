namespace CleanOpsAi.BuildingBlocks.Infrastructure.Events
{
	public class TaskAssignmentsGeneratedEvent
	{
		public List<ScheduleUpdateItem> Updates { get; set; } = new();
	}

	public class ScheduleUpdateItem
	{
		public Guid ScheduleId { get; set; }
		public DateOnly GeneratedToDate { get; set; }
	}
}
