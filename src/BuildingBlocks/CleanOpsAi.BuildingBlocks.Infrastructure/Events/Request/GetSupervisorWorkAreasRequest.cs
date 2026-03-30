namespace CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request
{
	public record GetSupervisorWorkAreasRequest
	{
		public string UserId { get; init; } = null!;
	}

	public record GetSupervisorWorkAreasResponse
	{
		public List<Guid> WorkAreaIds { get; init; } = new List<Guid>();
	}
}
