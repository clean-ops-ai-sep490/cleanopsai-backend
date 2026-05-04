using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request;
using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Response;
using CleanOpsAi.Modules.TaskOperations.Domain.Enums;
using CleanOpsAi.Modules.TaskOperations.Infrastructure.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CleanOpsAi.Modules.TaskOperations.Infrastructure.Consumers
{
    public class GetTaskDashboardConsumer : IConsumer<GetTaskDashboardRequest>
    {
        private readonly TaskOperationsDbContext _dbContext;

        public GetTaskDashboardConsumer(TaskOperationsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Consume(ConsumeContext<GetTaskDashboardRequest> context)
        {
            var message = context.Message;
            var limit = message.TopWorkerLimit <= 0 ? 5 : message.TopWorkerLimit;

            var taskQuery = _dbContext.TaskAssignments
                .AsNoTracking()
                .Where(x => !x.IsDeleted);

            var passedTasksToDate = await taskQuery
                .CountAsync(x =>
                    x.Status == TaskAssignmentStatus.Completed &&
                    x.ScheduledEndAt <= message.AsOfUtc);

            var totalTasksToDate = await taskQuery
                .CountAsync(x => x.ScheduledEndAt <= message.AsOfUtc);

            var nonPassedTasksToDate = totalTasksToDate - passedTasksToDate;

            var statusCounts = await taskQuery
                .GroupBy(x => x.Status)
                .Select(g => new TaskStatusCountDto
                {
                    Status = g.Key.ToString(),
                    TotalTasks = g.Count()
                })
                .ToListAsync();

            var monthlyCompletedQuery = taskQuery
                .Where(x =>
                    x.Status == TaskAssignmentStatus.Completed &&
                    x.ScheduledStartAt >= message.FromDate &&
                    x.ScheduledStartAt <= message.ToDate)
                .GroupBy(x => new { x.AssigneeId, x.AssigneeName })
                .Select(g => new DashboardWorkerTaskStatDto
                {
                    WorkerId = g.Key.AssigneeId,
                    WorkerName = g.Key.AssigneeName,
                    TotalTasks = g.Count()
                });

            var topWorkers = await monthlyCompletedQuery
                .OrderByDescending(x => x.TotalTasks)
                .ThenBy(x => x.WorkerName)
                .Take(limit)
                .ToListAsync();

            var lowWorkers = await monthlyCompletedQuery
                .OrderBy(x => x.TotalTasks)
                .ThenBy(x => x.WorkerName)
                .Take(limit)
                .ToListAsync();

            var workAreaTaskCounts = await taskQuery
                .GroupBy(x => x.WorkAreaId)
                .Select(g => new WorkAreaTaskCountDto
                {
                    WorkAreaId = g.Key,
                    TotalTasks = g.Count()
                })
                .ToListAsync();

            var automatedChecks = _dbContext.ComplianceChecks
                .AsNoTracking()
                .Where(x =>
                    !x.IsDeleted &&
                    x.Type == ComplianceCheckType.Automated &&
                    (x.Status == ComplianceCheckStatus.Passed || x.Status == ComplianceCheckStatus.Failed));

            var passedChecks = await automatedChecks.CountAsync(x => x.Status == ComplianceCheckStatus.Passed);
            var failedChecks = await automatedChecks.CountAsync(x => x.Status == ComplianceCheckStatus.Failed);
            var totalEvaluatedChecks = passedChecks + failedChecks;

            var passedPercentage = totalEvaluatedChecks == 0
                ? 0
                : Math.Round((double)passedChecks * 100 / totalEvaluatedChecks, 2);

            var failedPercentage = totalEvaluatedChecks == 0
                ? 0
                : Math.Round((double)failedChecks * 100 / totalEvaluatedChecks, 2);

            await context.RespondAsync(new GetTaskDashboardResponse
            {
                TotalTasksToDate = totalTasksToDate,
                PassedTasksToDate = passedTasksToDate,
                NonPassedTasksToDate = nonPassedTasksToDate,
                StatusCounts = statusCounts,
                TopWorkers = topWorkers,
                LowWorkers = lowWorkers,
                WorkAreaTaskCounts = workAreaTaskCounts,
                AiComplianceRate = new AiComplianceRateDto
                {
                    TotalAutomatedEvaluatedChecks = totalEvaluatedChecks,
                    PassedChecks = passedChecks,
                    FailedChecks = failedChecks,
                    PassedPercentage = passedPercentage,
                    FailedPercentage = failedPercentage
                }
            });
        }
    }
}
