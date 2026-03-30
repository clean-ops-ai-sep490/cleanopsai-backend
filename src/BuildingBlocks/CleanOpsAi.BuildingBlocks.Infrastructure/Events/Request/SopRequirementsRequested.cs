namespace CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request
{
	public record SopRequirementsRequested
	{
		public Guid TaskScheduleId { get; init; }
	}

	public record SopRequirementsIntegrated
	{
		public bool Found { get; init; }
		public List<Guid> RequiredSkillIds { get; init; } = new List<Guid>();
		public List<Guid> RequiredCertificationIds { get; init; } = new List<Guid>();
	}
}
