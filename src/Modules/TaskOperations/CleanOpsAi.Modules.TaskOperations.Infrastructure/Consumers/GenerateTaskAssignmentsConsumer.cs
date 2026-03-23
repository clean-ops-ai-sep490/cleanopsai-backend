using CleanOpsAi.BuildingBlocks.Infrastructure.Events;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.TaskOperations.Domain.Entities;
using CleanOpsAi.Modules.TaskOperations.Domain.Enums;
using MassTransit;
using Medo;

namespace CleanOpsAi.Modules.TaskOperations.Infrastructure.Consumers
{
	public class GenerateTaskAssignmentsConsumer(
	ITaskAssignmentRepository repo,
	IRecurrenceExpander expander)
	: IConsumer<GenerateTaskAssignmentsRequestedEvent>
	{
		public async Task Consume(
			ConsumeContext<GenerateTaskAssignmentsRequestedEvent> context)
		{
			Console.WriteLine("CONSUMER START");
			var msg = context.Message; 
			   
			Console.WriteLine($"Range: {msg.FromDate} - {msg.ToDate}");
 
			var scheduledTimes = expander.Expand(
				msg.RecurrenceType,
				msg.RecurrenceConfig,
				msg.FromDate,
				msg.ToDate); 

			var toInsert = new List<TaskAssignment>();

			foreach (var scheduledAt in scheduledTimes)
			{
				if (await repo.ExistsAsync(msg.ScheduleId, scheduledAt))
				{
					continue;
				}

				toInsert.Add(new TaskAssignment
				{
					Id = Uuid7.NewGuid(),
					TaskScheduleId = msg.ScheduleId,
					AssigneeId = msg.AssigneeId ?? Guid.Empty,
					OriginalAssigneeId = msg.AssigneeId ?? Guid.Empty,
					WorkAreaId = msg.WorkAreaId,
					ScheduledStartAt = scheduledAt,
					ScheduledEndAt = scheduledAt.AddMinutes(msg.DurationMinutes),
					Status = TaskAssignmentStatus.NotStarted,
					IsAdhocTask = false,
					Created = DateTime.UtcNow,
					CreatedBy = "system"
				});
			} 
			if (toInsert.Count > 0)
				await repo.BulkInsertAsync(toInsert);
		}
	}
}
