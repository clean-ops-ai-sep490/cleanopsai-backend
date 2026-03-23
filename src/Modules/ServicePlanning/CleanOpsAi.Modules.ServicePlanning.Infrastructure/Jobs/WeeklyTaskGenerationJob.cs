using CleanOpsAi.BuildingBlocks.Domain.Dtos;
using CleanOpsAi.BuildingBlocks.Infrastructure.Events;
using CleanOpsAi.Modules.ServicePlanning.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.ServicePlanning.Application.DTOs;
using MassTransit;
using Microsoft.Extensions.Options;
using Quartz;
using System.Text.Json;

namespace CleanOpsAi.Modules.ServicePlanning.Infrastructure.Jobs
{
	public class WeeklyTaskGenerationJob(
	IPublishEndpoint bus,
	ITaskScheduleService scheduleService,
	IOptions<JobOptionsConfig> options) : IJob
	{
		public async Task Execute(IJobExecutionContext context)
		{
			var opt = options.Value;
			var fromDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)); // 18
			var toDate = fromDate.AddDays(opt.LookAheadDays - 1); // 25

			var schedules = await scheduleService.GetActiveSchedulesAsync();

			foreach (var schedule in schedules)
			{
				var config = JsonSerializer.Deserialize<RecurrenceConfig>(
					schedule.RecurrenceConfig)!;

				var effectiveFrom = Max(fromDate, schedule.ContractStartDate);

				var effectiveTo = schedule.ContractEndDate.HasValue
					? Min(toDate, schedule.ContractEndDate.Value)
					: toDate;

				await bus.Publish(new GenerateTaskAssignmentsRequestedEvent(
					ScheduleId: schedule.Id,
					FromDate: effectiveFrom,
					ToDate: effectiveTo,
					AssigneeId: schedule.AssigneeId,
					WorkAreaId: schedule.WorkAreaId,
					RecurrenceConfig: config,
					RecurrenceType: schedule.RecurrenceType,
					DurationMinutes: schedule.DurationMinutes,
					Source: "auto"
				));
			}
		}

		private static DateOnly Max(DateOnly a, DateOnly b) => a > b ? a : b;

		private static DateOnly Min(DateOnly a, DateOnly b) => a < b ? a : b;
	}
}
