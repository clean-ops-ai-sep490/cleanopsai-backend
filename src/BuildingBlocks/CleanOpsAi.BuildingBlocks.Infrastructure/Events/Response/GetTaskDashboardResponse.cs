namespace CleanOpsAi.BuildingBlocks.Infrastructure.Events.Response
{
    public class GetTaskDashboardResponse
    {
        public int TotalTasksToDate { get; set; }
        public int PassedTasksToDate { get; set; }
        public int NonPassedTasksToDate { get; set; }
        public List<TaskStatusCountDto> StatusCounts { get; set; } = new();
        public List<DashboardWorkerTaskStatDto> TopWorkers { get; set; } = new();
        public List<DashboardWorkerTaskStatDto> LowWorkers { get; set; } = new();
        public List<WorkAreaTaskCountDto> WorkAreaTaskCounts { get; set; } = new();
        public AiComplianceRateDto AiComplianceRate { get; set; } = new();
    }

    public class TaskStatusCountDto
    {
        public string Status { get; set; } = null!;
        public int TotalTasks { get; set; }
    }

    public class DashboardWorkerTaskStatDto
    {
        public Guid WorkerId { get; set; }
        public string WorkerName { get; set; } = null!;
        public int TotalTasks { get; set; }
    }

    public class WorkAreaTaskCountDto
    {
        public Guid WorkAreaId { get; set; }
        public int TotalTasks { get; set; }
    }

    public class AiComplianceRateDto
    {
        public int TotalAutomatedEvaluatedChecks { get; set; }
        public int PassedChecks { get; set; }
        public int FailedChecks { get; set; }
        public double PassedPercentage { get; set; }
        public double FailedPercentage { get; set; }
    }
}
