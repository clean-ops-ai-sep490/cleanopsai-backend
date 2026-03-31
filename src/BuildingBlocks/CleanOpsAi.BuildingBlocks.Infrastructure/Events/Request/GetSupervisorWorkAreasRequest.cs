namespace CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request
{
	public record GetSupervisorWorkAreasRequest
	{
		public Guid WorkerId { get; init; }  
		public Guid WorkerIdTarget { get; init; }  
		public Guid WorkAreaId { get; init; } 

	}

	public record GetSupervisorWorkAreasResponse
	{
		public bool Found { get; init; }
		public Guid? SupervisorUserId { get; init; }
	}
}
