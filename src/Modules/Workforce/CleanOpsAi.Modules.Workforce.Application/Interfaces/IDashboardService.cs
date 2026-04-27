using CleanOpsAi.Modules.Workforce.Application.Dtos.Dashboard;
using CleanOpsAi.Modules.Workforce.Application.Dtos.WorkAreaSupervisors;
using CleanOpsAi.Modules.Workforce.Application.Dtos;

namespace CleanOpsAi.Modules.Workforce.Application.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardWorkerTotalResponse> GetWorkerTotalAsync(CancellationToken ct = default);
        Task<PagedResponse<WorkerGroupResponse>> GetWorkersBySupervisorAsync(
            Guid supervisorId,
            int pageNumber,
            int pageSize,
            CancellationToken ct = default);
        Task<DashboardTaskSummaryResponse> GetTaskSummaryAsync(CancellationToken ct = default);
        Task<List<DashboardStatusCountResponse>> GetTaskStatusCountsAsync(CancellationToken ct = default);
        Task<List<DashboardWorkerTaskResponse>> GetTopWorkersAsync(CancellationToken ct = default);
        Task<List<DashboardWorkerTaskResponse>> GetLowWorkersAsync(CancellationToken ct = default);
        Task<List<DashboardWorkAreaResponse>> GetWorkAreaStatsAsync(CancellationToken ct = default);
        Task<DashboardAiComplianceRateResponse> GetAiComplianceRateAsync(CancellationToken ct = default);
    }
}
