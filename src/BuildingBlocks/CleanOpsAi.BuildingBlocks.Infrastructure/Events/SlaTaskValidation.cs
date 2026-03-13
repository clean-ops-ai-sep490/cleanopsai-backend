namespace CleanOpsAi.BuildingBlocks.Infrastructure.Events
{
	public record SlaTaskValidationRequestedEvent
	{
		public Guid CorrelationId { get; init; }
		public Guid SlaTaskId { get; init; }
		public Guid SlaShiftId { get; init; }
		public Guid? WorkAreaDetailId { get; init; }
	}

	public record SlaTaskValidationCheckedEvent
	{
		public Guid CorrelationId { get; init; }
		public bool IsValid { get; init; }
		public string? FailureReason { get; init; }
	}
}
