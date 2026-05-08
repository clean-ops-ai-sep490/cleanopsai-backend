using CleanOpsAi.Modules.Scoring.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.Scoring.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.Scoring.Application.DTOs.Request;
using CleanOpsAi.Modules.Scoring.Infrastructure.Options;
using CleanOpsAi.Modules.Scoring.IntegrationEvents;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CleanOpsAi.Modules.Scoring.Infrastructure.Consumers
{
	public class ScoringAnnotationApprovedConsumer : IConsumer<ScoringAnnotationApprovedEvent>
	{
		private readonly IScoringJobRepository _repository;
		private readonly IScoringJobService _scoringJobService;
		private readonly IOptions<ScoringRetrainOptions> _options;
		private readonly ILogger<ScoringAnnotationApprovedConsumer> _logger;

		public ScoringAnnotationApprovedConsumer(
			IScoringJobRepository repository,
			IScoringJobService scoringJobService,
			IOptions<ScoringRetrainOptions> options,
			ILogger<ScoringAnnotationApprovedConsumer> logger)
		{
			_repository = repository;
			_scoringJobService = scoringJobService;
			_options = options;
			_logger = logger;
		}

		public async Task Consume(ConsumeContext<ScoringAnnotationApprovedEvent> context)
		{
			var config = _options.Value;
			var threshold = Math.Max(0, config.AutoBatchThreshold);
			if (!config.AutoTriggerEnabled || threshold == 0)
			{
				_logger.LogDebug(
					"Skip scoring retrain auto trigger for annotation {AnnotationId}. AutoTriggerEnabled={AutoTriggerEnabled}, Threshold={Threshold}.",
					context.Message.AnnotationId,
					config.AutoTriggerEnabled,
					threshold);
				return;
			}

			try
			{
				if (await _repository.HasActiveRetrainBatchAsync(context.CancellationToken))
				{
					_logger.LogInformation(
						"Skip scoring retrain auto trigger after annotation {AnnotationId} because a retrain batch is already queued or running.",
						context.Message.AnnotationId);
					return;
				}

				var now = DateTime.UtcNow;
				var latestBatch = await _repository.GetLatestRetrainBatchAsync(context.CancellationToken);
				var sourceWindowFromUtc = latestBatch?.RequestedAtUtc ?? now.AddDays(-Math.Max(1, config.LookbackDays));
				var approvedCount = await _repository.CountApprovedAnnotationCandidatesForRetrainAsync(
					sourceWindowFromUtc,
					context.CancellationToken);

				if (approvedCount < threshold)
				{
					_logger.LogInformation(
						"Skip scoring retrain auto trigger after annotation {AnnotationId}. Approved annotations since {SourceWindowFromUtc}: {ApprovedCount}/{Threshold}.",
						context.Message.AnnotationId,
						sourceWindowFromUtc,
						approvedCount,
						threshold);
					return;
				}

				var maxSamplesPerBatch = Math.Clamp(Math.Max(config.MaxSamplesPerBatch, threshold), 1, 5000);
				var batch = await _scoringJobService.TriggerRetrainAsync(new TriggerScoringRetrainRequest
				{
					LookbackDays = Math.Max(1, config.LookbackDays),
					MinReviewedSamples = Math.Max(1, config.MinReviewedSamples),
					MinApprovedAnnotations = threshold,
					MaxSamplesPerBatch = maxSamplesPerBatch,
					UseLastBatchTime = true,
				}, context.CancellationToken);

				_logger.LogInformation(
					"Auto-triggered scoring retrain batch {BatchId} after annotation {AnnotationId} with {ApprovedAnnotationCount} approved annotations.",
					batch.BatchId,
					context.Message.AnnotationId,
					batch.ApprovedAnnotationCount);
			}
			catch (Exception ex)
			{
				_logger.LogError(
					ex,
					"Failed to auto-trigger scoring retrain after approved annotation {AnnotationId}.",
					context.Message.AnnotationId);
			}
		}
	}
}
