using CleanOpsAi.BuildingBlocks.Infrastructure.Events;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services;
using MassTransit;
using Microsoft.Extensions.Logging; 

namespace CleanOpsAi.Modules.TaskOperations.Infrastructure.Consumers
{
	public class PpeCheckCompletedConsumer : IConsumer<PpeCheckCompletedEvent>
	{
		private readonly ITaskStepExecutionService _service;
		private readonly ILogger<PpeCheckCompletedConsumer> _logger;

		public PpeCheckCompletedConsumer(
			ITaskStepExecutionService service,
			ILogger<PpeCheckCompletedConsumer> logger)
		{
			_service = service;
			_logger = logger;
		}

		public async Task Consume(ConsumeContext<PpeCheckCompletedEvent> context)
		{
			var evt = context.Message;

			if (evt is null)
			{
				_logger.LogWarning("[PpeCheckCompletedConsumer] Received null message.");
				return;
			}

			_logger.LogInformation(
				"[PpeCheckCompletedConsumer] Received PpeCheckCompletedEvent for TaskStepExecutionId={Id}, Status={Status}",
				evt.TaskStepExecutionId, evt.Status);

			try
			{
				await _service.ApplyPpeCheckResultAsync(evt, context.CancellationToken);

				_logger.LogInformation(
					"[PpeCheckCompletedConsumer] Successfully processed PpeCheckCompletedEvent for TaskStepExecutionId={Id}",
					evt.TaskStepExecutionId);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex,
					"[PpeCheckCompletedConsumer] Failed to process PpeCheckCompletedEvent for TaskStepExecutionId={Id}",
					evt.TaskStepExecutionId);
				throw;
			}
		}
	}
}
