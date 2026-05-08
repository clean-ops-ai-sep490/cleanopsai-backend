using CleanOpsAi.BuildingBlocks.Infrastructure.Events;
using CleanOpsAi.Modules.Scoring.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.Scoring.Application.DTOs.Request;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CleanOpsAi.Modules.Scoring.Infrastructure.Consumers
{
	/// <summary>
	/// Consumes <see cref="AiScoringRequestedEvent"/> published by TaskOperations module.
	/// Submits the scoring job to <see cref="IScoringJobService"/>, keeping the service
	/// exclusively within the Scoring module boundary.
	/// </summary>
	public class AiScoringRequestedConsumer : IConsumer<AiScoringRequestedEvent>
	{
		private readonly IScoringJobService _scoringJobService;
		private readonly ILogger<AiScoringRequestedConsumer> _logger;

		public AiScoringRequestedConsumer(
			IScoringJobService scoringJobService,
			ILogger<AiScoringRequestedConsumer> logger)
		{
			_scoringJobService = scoringJobService;
			_logger = logger;
		}

		public async Task Consume(ConsumeContext<AiScoringRequestedEvent> context)
		{
			var message = context.Message;
			
			_logger.LogInformation(
				"Received AiScoringRequestedEvent for ComplianceCheck {ComplianceCheckId}, " +
				"TaskStepExecution {ExecutionId}, ImageCount={ImageCount}",
				message.ComplianceCheckId,
				message.TaskStepExecutionId,
				message.ImageUrls.Count);

			try
			{ 
				var jobRequest = new CreateScoringJobRequest
				{
					RequestId = message.ComplianceCheckId.ToString(),
					EnvironmentKey = message.EnvironmentKey,
					ImageUrls = message.ImageUrls,
					SubmittedByUserId = message.SubmittedByUserId
				};

				// ── Submit to scoring job service ─────────────────────────────────
				var jobResponse = await _scoringJobService.SubmitAsync(
					jobRequest,
					context.CancellationToken);

				_logger.LogInformation(
					"Scoring job {JobId} queued (Status={Status}) for compliance check {ComplianceCheckId}",
					jobResponse.JobId,
					jobResponse.Status,
					message.ComplianceCheckId);
			}
			catch (Exception ex)
			{
				var retryAttempt = context.GetRetryAttempt();
				_logger.LogError(
					ex,
					"Failed processing AiScoringRequestedEvent for ComplianceCheck {ComplianceCheckId}. " +
					"RetryAttempt={RetryAttempt}",
					message.ComplianceCheckId,
					retryAttempt);
				 
				throw;
			}
		}
	}
}
