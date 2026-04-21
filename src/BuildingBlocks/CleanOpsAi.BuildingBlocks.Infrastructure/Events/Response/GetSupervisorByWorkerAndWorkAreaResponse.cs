namespace CleanOpsAi.BuildingBlocks.Infrastructure.Events.Response
{
    public record GetSupervisorByWorkerAndWorkAreaResponse
    {
        public Guid? SupervisorId { get; init; }
        public string? SupervisorName { get; init; }
        public bool Found { get; init; }
    }
}
