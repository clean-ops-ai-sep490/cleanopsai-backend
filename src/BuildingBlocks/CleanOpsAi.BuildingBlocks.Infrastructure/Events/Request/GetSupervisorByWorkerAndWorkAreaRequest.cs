namespace CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request
{
    public record GetSupervisorByWorkerAndWorkAreaRequest
    {
        public Guid WorkAreaId { get; init; }
        public Guid WorkerId { get; init; }
    }
}
