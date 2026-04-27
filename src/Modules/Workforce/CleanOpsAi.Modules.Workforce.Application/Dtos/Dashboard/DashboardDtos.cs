namespace CleanOpsAi.Modules.Workforce.Application.Dtos.Dashboard
{
    public class DashboardWorkerTotalResponse
    {
        public int TotalWorkers { get; set; }
    }

    public class DashboardTaskSummaryResponse
    {
        public int TotalTasksToDate { get; set; }
        public int PassedTasksToDate { get; set; }
        public int NonPassedTasksToDate { get; set; }
    }

    public class DashboardStatusCountResponse
    {
        public string Status { get; set; } = null!;
        public int TotalTasks { get; set; }
    }

    public class DashboardWorkerTaskResponse
    {
        public Guid WorkerId { get; set; }
        public string WorkerName { get; set; } = null!;
        public int TotalTasks { get; set; }
    }

    public class DashboardWorkAreaResponse
    {
        public Guid WorkAreaId { get; set; }
        public string? WorkAreaName { get; set; }
        public string? DisplayLocation { get; set; }
        public int TotalWorkers { get; set; }
        public int TotalTasks { get; set; }
    }

    public class DashboardAiComplianceRateResponse
    {
        public int TotalAutomatedEvaluatedChecks { get; set; }
        public int PassedChecks { get; set; }
        public int FailedChecks { get; set; }
        public double PassedPercentage { get; set; }
        public double FailedPercentage { get; set; }
    }
}
