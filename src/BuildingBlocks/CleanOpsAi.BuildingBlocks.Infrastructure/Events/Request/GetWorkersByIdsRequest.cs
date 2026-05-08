namespace CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request
{
	public record GetWorkersByIdsRequest {
		public List<Guid> WorkerIds { get; init; } = new List<Guid>();
	}

	public record GetWorkersByIdsResponse {
		public List<WorkerDto> Workers { get; init; } = new List<WorkerDto>();
	};

	public record WorkerDto
	{
		public Guid Id { get; init; }
		public Guid UserId { get; init; }
		public string FullName { get; init; } = null!;

	}
}
