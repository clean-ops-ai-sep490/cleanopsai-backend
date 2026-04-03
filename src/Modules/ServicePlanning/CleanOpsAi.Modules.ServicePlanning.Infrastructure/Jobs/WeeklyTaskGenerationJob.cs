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
	IBus bus,
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

				Console.WriteLine($"ScheduleId: {schedule.Id} - Name: {schedule.AssigneeName}");
				Console.WriteLine($"Location: {schedule.Id} - Name: {schedule.DisplayLocation}");

				await bus.Publish(new GenerateTaskAssignmentsRequestedEvent
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
		}

		private static DateOnly Max(DateOnly a, DateOnly b) => a > b ? a : b;

		private static DateOnly Min(DateOnly a, DateOnly b) => a < b ? a : b;
	}
}
