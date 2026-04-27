namespace CleanOpsAi.Modules.TaskOperations.Application.DTOs.Response
{
    public class TaskDashboardResponse
    {
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int NotStartedTasks { get; set; }
        public List<WorkerTaskStatsDto> TopWorkers { get; set; } = new();
        public List<WorkerTaskStatsDto> LowWorkers { get; set; } = new();
    }
}
