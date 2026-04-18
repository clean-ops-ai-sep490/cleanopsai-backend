namespace CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request
{
	public record GetWorkersByUserIdsRequest
	{
		public List<Guid> UserIds { get; init; } = new();
	}

	public record GetWorkersByUserIdsResponse
	{
		public List<WorkerSummaryByUserIdDto> Workers { get; init; } = new();
	}

	public record WorkerSummaryByUserIdDto
	{
		public Guid UserId { get; init; }
		public Guid WorkerId { get; init; }
		public string FullName { get; init; } = null!;
	}
}
