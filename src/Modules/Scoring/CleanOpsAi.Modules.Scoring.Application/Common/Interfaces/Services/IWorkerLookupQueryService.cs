namespace CleanOpsAi.Modules.Scoring.Application.Common.Interfaces.Services
{
	public interface IWorkerLookupQueryService
	{
		Task<IReadOnlyCollection<WorkerLookupItem>> GetWorkersByUserIdsAsync(
			IReadOnlyCollection<Guid> userIds,
			CancellationToken ct = default);
	}

	public record WorkerLookupItem
	{
		public Guid UserId { get; init; }
		public Guid WorkerId { get; init; }
		public string FullName { get; init; } = null!;
	}
}
