using CleanOpsAi.Modules.Scoring.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.Scoring.IntegrationEvents;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CleanOpsAi.Modules.Scoring.Infrastructure.Consumers
{
	public class ScoringJobRequestedConsumer : IConsumer<ScoringJobRequestedEvent>
	{
		private readonly IScoringJobService _scoringJobService;
		private readonly ILogger<ScoringJobRequestedConsumer> _logger;

		public ScoringJobRequestedConsumer(
			IScoringJobService scoringJobService,
			ILogger<ScoringJobRequestedConsumer> logger)
		{
			_scoringJobService = scoringJobService;
			_logger = logger;
		}

		public async Task Consume(ConsumeContext<ScoringJobRequestedEvent> context)
		{
			var message = context.Message;
			try
			{
				await _scoringJobService.ProcessQueuedJobAsync(
					message.JobId,
					message.EnvironmentKey,
					message.ImageUrls,
					message.IncludeVisualizations,
					context.CancellationToken);
			}
			catch (Exception ex)
			{
				var retryAttempt = context.GetRetryAttempt();
				_logger.LogError(
					ex,
					"Failed processing scoring job {JobId}. RetryAttempt={RetryAttempt}",
					message.JobId,
					retryAttempt);

				if (retryAttempt >= 2)
				{
					await _scoringJobService.MarkFailedAsync(message.JobId, ex.Message, context.CancellationToken);
				}

				throw;
			}
		}
	}
}
