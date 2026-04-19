using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.TaskOperations.IntegrationEvents;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CleanOpsAi.Scoring.Worker.Consumers
{
	public class PpeCheckRequestedConsumer : IConsumer<PpeCheckRequestedEvent>
	{
		private readonly ITaskStepExecutionService _taskStepExecutionService;
		private readonly ILogger<PpeCheckRequestedConsumer> _logger;

		public PpeCheckRequestedConsumer(
			ITaskStepExecutionService taskStepExecutionService,
			ILogger<PpeCheckRequestedConsumer> logger)
		{
			_taskStepExecutionService = taskStepExecutionService;
			_logger = logger;
		}

		public async Task Consume(ConsumeContext<PpeCheckRequestedEvent> context)
		{
			var message = context.Message;
			try
			{
				await _taskStepExecutionService.ProcessQueuedPpeCheckAsync(
					message.TaskStepExecutionId,
					context.CancellationToken);
			}
			catch (Exception ex)
			{
				_logger.LogError(
					ex,
					"Failed processing PPE check job for taskStepExecutionId {TaskStepExecutionId}",
					message.TaskStepExecutionId);
				throw;
			}
		}
	}
}
