namespace CleanOpsAi.Modules.TaskOperations.Application.DTOs.Response
{
    public class WorkerTaskStatsDto
    {
        public Guid WorkerId { get; set; }
        public string WorkerName { get; set; } = null!;
        public int TotalTasks { get; set; }
    }
}
