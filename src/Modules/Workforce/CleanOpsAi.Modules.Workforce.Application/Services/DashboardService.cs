using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request;
using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Response;
using CleanOpsAi.Modules.Workforce.Application.Dtos;
using CleanOpsAi.Modules.Workforce.Application.Dtos.Dashboard;
using CleanOpsAi.Modules.Workforce.Application.Dtos.WorkAreaSupervisors;
using CleanOpsAi.Modules.Workforce.Application.Interfaces;
using MassTransit;

namespace CleanOpsAi.Modules.Workforce.Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IWorkerRepository _workerRepository;
        private readonly IWorkAreaSupervisorService _workAreaSupervisorService;
        private readonly IWorkAreaSupervisorRepository _workAreaSupervisorRepository;
        private readonly IRequestClient<GetTaskDashboardRequest> _taskDashboardClient;
        private readonly IRequestClient<GetWorkAreasByIdsRequest> _workAreaClient;
        private readonly IDateTimeProvider _dateTimeProvider;

        public DashboardService(
            IWorkerRepository workerRepository,
            IWorkAreaSupervisorService workAreaSupervisorService,
            IWorkAreaSupervisorRepository workAreaSupervisorRepository,
            IRequestClient<GetTaskDashboardRequest> taskDashboardClient,
            IRequestClient<GetWorkAreasByIdsRequest> workAreaClient,
            IDateTimeProvider dateTimeProvider)
        {
            _workerRepository = workerRepository;
            _workAreaSupervisorService = workAreaSupervisorService;
            _workAreaSupervisorRepository = workAreaSupervisorRepository;
            _taskDashboardClient = taskDashboardClient;
            _workAreaClient = workAreaClient;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<DashboardWorkerTotalResponse> GetWorkerTotalAsync(CancellationToken ct = default)
        {
            var totalWorkers = await _workerRepository.CountAllActiveAsync();

            return new DashboardWorkerTotalResponse
            {
                TotalWorkers = totalWorkers
            };
        }

        public async Task<PagedResponse<WorkerGroupResponse>> GetWorkersBySupervisorAsync(
            Guid supervisorId,
            int pageNumber,
            int pageSize,
            CancellationToken ct = default)
        {
            return await _workAreaSupervisorService.GetUniqueWorkersBySupervisorPagingAsync(
                supervisorId,
                pageNumber,
                pageSize);
        }

        public async Task<DashboardTaskSummaryResponse> GetTaskSummaryAsync(CancellationToken ct = default)
        {
            var taskDashboard = await GetTaskDashboardMessageAsync(ct);

            return new DashboardTaskSummaryResponse
            {
                TotalTasksToDate = taskDashboard.TotalTasksToDate,
                PassedTasksToDate = taskDashboard.PassedTasksToDate,
                NonPassedTasksToDate = taskDashboard.NonPassedTasksToDate
            };
        }

        public async Task<List<DashboardStatusCountResponse>> GetTaskStatusCountsAsync(CancellationToken ct = default)
        {
            var taskDashboard = await GetTaskDashboardMessageAsync(ct);

            return taskDashboard.StatusCounts
                .Select(x => new DashboardStatusCountResponse
                {
                    Status = x.Status,
                    TotalTasks = x.TotalTasks
                })
                .OrderBy(x => x.Status)
                .ToList();
        }

        public async Task<List<DashboardWorkerTaskResponse>> GetTopWorkersAsync(CancellationToken ct = default)
        {
            var taskDashboard = await GetTaskDashboardMessageAsync(ct);

            return taskDashboard.TopWorkers
                .Take(5)
                .Select(x => new DashboardWorkerTaskResponse
                {
                    WorkerId = x.WorkerId,
                    WorkerName = x.WorkerName,
                    TotalTasks = x.TotalTasks
                })
                .ToList();
        }

        public async Task<List<DashboardWorkerTaskResponse>> GetLowWorkersAsync(CancellationToken ct = default)
        {
            var taskDashboard = await GetTaskDashboardMessageAsync(ct);

            return taskDashboard.LowWorkers
                .Take(5)
                .Select(x => new DashboardWorkerTaskResponse
                {
                    WorkerId = x.WorkerId,
                    WorkerName = x.WorkerName,
                    TotalTasks = x.TotalTasks
                })
                .ToList();
        }

        public async Task<List<DashboardWorkAreaResponse>> GetWorkAreaStatsAsync(CancellationToken ct = default)
        {
            var taskDashboard = await GetTaskDashboardMessageAsync(ct);
            var assignments = await _workAreaSupervisorRepository.GetAllAsync();

            var workerCountsByArea = assignments
                .Where(x => !x.IsDeleted && x.WorkAreaId.HasValue && x.WorkerId.HasValue)
                .GroupBy(x => x.WorkAreaId!.Value)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.WorkerId!.Value).Distinct().Count());

            var taskCountsByArea = taskDashboard.WorkAreaTaskCounts
                .GroupBy(x => x.WorkAreaId)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.TotalTasks));

            var workAreaIds = workerCountsByArea.Keys
                .Union(taskCountsByArea.Keys)
                .Distinct()
                .ToList();

            var workAreaLookup = await GetWorkAreaLookupAsync(workAreaIds, ct);

            return workAreaIds
                .Select(workAreaId =>
                {
                    workAreaLookup.TryGetValue(workAreaId, out var areaInfo);

                    return new DashboardWorkAreaResponse
                    {
                        WorkAreaId = workAreaId,
                        WorkAreaName = areaInfo?.WorkAreaName,
                        DisplayLocation = areaInfo?.DisplayLocation,
                        TotalWorkers = workerCountsByArea.GetValueOrDefault(workAreaId),
                        TotalTasks = taskCountsByArea.GetValueOrDefault(workAreaId)
                    };
                })
                .OrderByDescending(x => x.TotalTasks)
                .ThenBy(x => x.WorkAreaName)
                .ToList();
        }

        public async Task<DashboardAiComplianceRateResponse> GetAiComplianceRateAsync(CancellationToken ct = default)
        {
            var taskDashboard = await GetTaskDashboardMessageAsync(ct);

            return new DashboardAiComplianceRateResponse
            {
                TotalAutomatedEvaluatedChecks = taskDashboard.AiComplianceRate.TotalAutomatedEvaluatedChecks,
                PassedChecks = taskDashboard.AiComplianceRate.PassedChecks,
                FailedChecks = taskDashboard.AiComplianceRate.FailedChecks,
                PassedPercentage = taskDashboard.AiComplianceRate.PassedPercentage,
                FailedPercentage = taskDashboard.AiComplianceRate.FailedPercentage
            };
        }

        private async Task<GetTaskDashboardResponse> GetTaskDashboardMessageAsync(CancellationToken ct)
        {
            var now = _dateTimeProvider.UtcNow;
            var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var monthEnd = monthStart.AddMonths(1).AddTicks(-1);

            var response = await _taskDashboardClient
                .GetResponse<GetTaskDashboardResponse>(
                    new GetTaskDashboardRequest
                    {
                        AsOfUtc = now,
                        FromDate = monthStart,
                        ToDate = monthEnd,
                        TopWorkerLimit = 5
                    },
                    cancellationToken: ct);

            return response.Message;
        }

        private async Task<Dictionary<Guid, WorkAreaWithLocationDto>> GetWorkAreaLookupAsync(
            List<Guid> workAreaIds,
            CancellationToken ct)
        {
            if (workAreaIds.Count == 0)
            {
                return new Dictionary<Guid, WorkAreaWithLocationDto>();
            }

            var workAreaResponse = await _workAreaClient
                .GetResponse<GetWorkAreasByIdsResponse>(
                    new GetWorkAreasByIdsRequest
                    {
                        WorkAreaIds = workAreaIds
                    },
                    cancellationToken: ct);

            return workAreaResponse.Message.Items
                .GroupBy(x => x.WorkAreaId)
                .ToDictionary(g => g.Key, g => g.First());
        }
    }
}
