using CleanOpsAi.BuildingBlocks.Domain.Dtos;
using CleanOpsAi.BuildingBlocks.Infrastructure.Events;
using CleanOpsAi.Modules.ServicePlanning.Application.Common.Interfaces.Events;
using CleanOpsAi.Modules.ServicePlanning.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.ServicePlanning.Application.DTOs; 
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using System.Text.Json;

namespace CleanOpsAi.Modules.ServicePlanning.Infrastructure.Jobs
{
	public class WeeklyTaskGenerationJob : IJob
	{
		private readonly ITaskScheduleEventService _taskScheduleEventService;
		private readonly ITaskScheduleService _taskScheduleService;
		private readonly ILogger<WeeklyTaskGenerationJob> _logger;
		private readonly IOptions<JobOptionsConfig> _options;

		public WeeklyTaskGenerationJob(ITaskScheduleEventService taskScheduleEventService, ITaskScheduleService taskScheduleService, ILogger<WeeklyTaskGenerationJob> logger,
			IOptions<JobOptionsConfig> options)
		{
			_taskScheduleEventService = taskScheduleEventService;
			_taskScheduleService = taskScheduleService;
			_logger = logger;
			_options = options;
		}

		public async Task Execute(IJobExecutionContext context)
		{  
			try
			{
				var opt = _options.Value;

				var fromDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
				var toDate = fromDate.AddDays(opt.LookAheadDays - 1);

				var schedules = await _taskScheduleService.GetActiveSchedulesAsync();

				// 1. Map → items
				var items = new List<GenerateTaskAssignmentItem>();

				foreach (var schedule in schedules)
				{
					var config = JsonSerializer.Deserialize<RecurrenceConfig>(
						schedule.RecurrenceConfig)!;

					var effectiveFrom = Max(fromDate, schedule.ContractStartDate);

					var effectiveTo = schedule.ContractEndDate.HasValue
						? Min(toDate, schedule.ContractEndDate.Value)
						: toDate;

					items.Add(new GenerateTaskAssignmentItem
					{
						ScheduleId = schedule.Id,
						AssigneeId = schedule.AssigneeId,
						WorkAreaId = schedule.WorkAreaId,
						FromDate = effectiveFrom,
						ToDate = effectiveTo,
						RecurrenceType = schedule.RecurrenceType,
						RecurrenceConfig = config,
						DurationMinutes = schedule.DurationMinutes,
						AssigneeName = schedule.AssigneeName,
						DisplayLocation = schedule.DisplayLocation,
						Source = "auto"
					});
				}

				// 2. Chunk (100 items)
				var chunks = items.Chunk(100).ToList();
				

				foreach (var chunk in chunks)
				{
					await _taskScheduleEventService.RequestGenerateAssignments(
						new GenerateTaskAssignmentsRequestedEvent
						{
							Items = chunk.ToList()
						},
						context.CancellationToken);
				}
				_logger.LogInformation("Generated {Count} items across {Chunks} chunks",
					items.Count, chunks.Count());

			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "WeeklyTaskGenerationJob failed");
				throw; // Quartz cần rethrow để biết job failed
			}
		}

		private static DateOnly Max(DateOnly a, DateOnly b) => a > b ? a : b;

		private static DateOnly Min(DateOnly a, DateOnly b) => a < b ? a : b;
	}
}
