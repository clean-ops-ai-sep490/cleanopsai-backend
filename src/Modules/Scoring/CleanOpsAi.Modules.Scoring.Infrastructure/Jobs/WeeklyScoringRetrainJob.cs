using CleanOpsAi.BuildingBlocks.Application.Interfaces.Messaging;
using CleanOpsAi.Modules.Scoring.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.Scoring.Domain.Entities;
using CleanOpsAi.Modules.Scoring.IntegrationEvents;
using CleanOpsAi.Modules.Scoring.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using System.Text.Json.Nodes;

namespace CleanOpsAi.Modules.Scoring.Infrastructure.Jobs
{
	public class WeeklyScoringRetrainJob : IJob
	{
		private readonly IScoringJobRepository _repository;
		private readonly IEventBus _eventBus;
		private readonly IOptions<ScoringRetrainOptions> _options;
		private readonly ILogger<WeeklyScoringRetrainJob> _logger;

		public WeeklyScoringRetrainJob(
			IScoringJobRepository repository,
			IEventBus eventBus,
			IOptions<ScoringRetrainOptions> options,
			ILogger<WeeklyScoringRetrainJob> logger)
		{
			_repository = repository;
			_eventBus = eventBus;
			_options = options;
			_logger = logger;
		}

		public async Task Execute(IJobExecutionContext context)
		{
			var config = _options.Value;
			var now = DateTime.UtcNow;
			var lookbackDays = Math.Max(1, config.LookbackDays);
			var sinceUtc = now.AddDays(-lookbackDays);
			var take = Math.Clamp(config.MaxSamplesPerBatch, 1, 5000);

			var reviewedResults = await _repository.GetReviewedResultsForRetrainAsync(sinceUtc, take, context.CancellationToken);
			if (reviewedResults.Count < Math.Max(1, config.MinReviewedSamples))
			{
				_logger.LogInformation(
					"Skip weekly scoring retrain trigger. Reviewed samples in window: {Count}, required: {Required}, since: {SinceUtc}",
					reviewedResults.Count,
					config.MinReviewedSamples,
					sinceUtc);
				return;
			}

			var samples = reviewedResults
				.Select(MapSample)
				.ToList();

			var batchId = Guid.NewGuid();
			await _eventBus.PublishAsync(new ScoringRetrainRequestedEvent
			{
				BatchId = batchId,
				RequestedAtUtc = now,
				SourceWindowFromUtc = sinceUtc,
				ReviewedSampleCount = samples.Count,
				Samples = samples,
			}, context.CancellationToken);

			_logger.LogInformation(
				"Published scoring retrain request {BatchId} with {SampleCount} reviewed samples.",
				batchId,
				samples.Count);
		}

		private static ScoringRetrainSampleItem MapSample(ScoringJobResult result)
		{
			var reviewedAtUtc = result.LastModified;
			var reviewedByEmail = default(string);

			if (!string.IsNullOrWhiteSpace(result.PayloadJson))
			{
				try
				{
					var root = JsonNode.Parse(result.PayloadJson) as JsonObject;
					var review = root?["human_review"] as JsonObject;
					var reviewedAtRaw = review?["reviewed_at_utc"]?.GetValue<string>();
					if (DateTime.TryParse(reviewedAtRaw, out var parsedReviewedAt))
					{
						reviewedAtUtc = DateTime.SpecifyKind(parsedReviewedAt, DateTimeKind.Utc);
					}
					reviewedByEmail = review?["reviewed_by_email"]?.GetValue<string>();
				}
				catch
				{
					// Keep fallback metadata when payload cannot be parsed.
				}
			}

			return new ScoringRetrainSampleItem
			{
				ResultId = result.Id,
				JobId = result.ScoringJobId,
				RequestId = result.ScoringJob.RequestId,
				EnvironmentKey = result.ScoringJob.EnvironmentKey,
				SourceType = result.SourceType,
				Source = result.Source,
				ReviewedVerdict = result.Verdict,
				ReviewedAtUtc = reviewedAtUtc,
				ReviewedByEmail = reviewedByEmail,
			};
		}
	}
}
