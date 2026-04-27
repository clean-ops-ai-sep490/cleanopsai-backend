namespace CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request
{
    public class GetTaskDashboardRequest
    {
        public DateTime AsOfUtc { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int TopWorkerLimit { get; set; } = 5;
    }
}
