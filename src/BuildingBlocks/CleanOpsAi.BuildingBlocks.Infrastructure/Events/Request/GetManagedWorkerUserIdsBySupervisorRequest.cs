namespace CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request
{
	public record GetManagedWorkerUserIdsBySupervisorRequest
	{
		public Guid SupervisorUserId { get; init; }
	}

	public record GetManagedWorkerUserIdsBySupervisorResponse
	{
		public List<Guid> WorkerUserIds { get; init; } = new();
	}
}
