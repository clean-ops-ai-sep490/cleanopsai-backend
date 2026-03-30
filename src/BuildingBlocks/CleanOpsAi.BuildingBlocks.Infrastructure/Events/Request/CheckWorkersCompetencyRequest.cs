namespace CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request
{
	public record GetQualifiedWorkersRequested
	{
		public List<Guid> RequiredSkillIds { get; init; } = new List<Guid>();
		public List<Guid> RequiredCertificationIds { get; init; } = new List<Guid>();
	}

	// Dùng cho GetCandidates
	public record GetQualifiedWorkersIntegrated
	{
		public List<Guid> QualifiedWorkerIds { get; init; } = new List<Guid>();
	}

	public record CheckSingleWorkerCompetencyRequested
	{
		public Guid WorkerId { get; init; }
		public List<Guid> RequiredSkillIds { get; init; } = new List<Guid>();
		public List<Guid> RequiredCertificationIds { get; init; } = new List<Guid>();
	}

	// Dùng cho CreateSwapRequest
	public record CheckSingleWorkerCompetencyIntegrated
	{
		public bool IsQualified { get; init; }
	}
}
