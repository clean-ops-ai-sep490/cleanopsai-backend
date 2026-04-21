using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CleanOpsAi.Modules.TaskOperations.Infrastructure.Consumers
{
	public class ScoringCompletedConsumer : IConsumer<ScoringCompletedEvent>
	{
		private readonly IComplianceCheckService _complianceCheckService;
		private readonly ILogger<ScoringCompletedConsumer> _logger;

		public ScoringCompletedConsumer(
			IComplianceCheckService complianceCheckService,
			ILogger<ScoringCompletedConsumer> logger)
		{
			_complianceCheckService = complianceCheckService;
			_logger = logger;
		}

		public async Task Consume(ConsumeContext<ScoringCompletedEvent> context)
		{
			var evt = context.Message;
			 
			if (evt == null || evt.Results == null)
			{
				_logger.LogWarning("[ScoringCompletedConsumer] Received a null message or empty results. RequestId: {RequestId}", evt?.RequestId);
				return;
			}
			 
			_logger.LogInformation(
				"[ScoringCompletedConsumer] Received ScoringCompletedEvent for RequestId={RequestId}, ResultCount={Count}",
				evt.RequestId,
				evt.Results.Count);

			try
			{ 
				await _complianceCheckService.ApplyScoringResultsAsync(evt, context.CancellationToken);
				 
				_logger.LogInformation(
					"[ScoringCompletedConsumer] Successfully processed ScoringCompletedEvent for RequestId={RequestId}",
					evt.RequestId);
			}
			catch (Exception ex)
			{ 
				_logger.LogError(ex,
					"[ScoringCompletedConsumer] Failed to process ScoringCompletedEvent for RequestId={RequestId}",
					evt.RequestId);
				 
				throw;
			}
		}
	}
}
