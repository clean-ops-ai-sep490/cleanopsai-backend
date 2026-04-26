using CleanOpsAi.BuildingBlocks.Application.Interfaces.Messaging;
using CleanOpsAi.Modules.Scoring.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.Scoring.Application.DTOs.Request;
using CleanOpsAi.Modules.Scoring.Domain.Entities;
using CleanOpsAi.Modules.Scoring.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;

namespace CleanOpsAi.Modules.Scoring.Infrastructure.Jobs
{
	public class WeeklyScoringRetrainJob : IJob
	{
		private readonly IScoringJobService _scoringJobService;
		private readonly IOptions<ScoringRetrainOptions> _options;
		private readonly ILogger<WeeklyScoringRetrainJob> _logger;

		public WeeklyScoringRetrainJob(
			IScoringJobService scoringJobService,
			IOptions<ScoringRetrainOptions> options,
			ILogger<WeeklyScoringRetrainJob> logger)
		{
			_scoringJobService = scoringJobService;
			_options = options;
			_logger = logger;
		}

		public async Task Execute(IJobExecutionContext context)
		{
			var config = _options.Value;
			try
			{
				var batch = await _scoringJobService.TriggerRetrainAsync(new TriggerScoringRetrainRequest
				{
					LookbackDays = config.LookbackDays,
					MinReviewedSamples = config.MinReviewedSamples,
					MaxSamplesPerBatch = config.MaxSamplesPerBatch,
				}, context.CancellationToken);

				_logger.LogInformation(
					"Published weekly scoring retrain request {BatchId} with {SampleCount} reviewed samples.",
					batch.BatchId,
					batch.ReviewedSampleCount);
			}
			catch (InvalidOperationException ex)
			{
				_logger.LogInformation(ex, "Skip weekly scoring retrain trigger because retrain preconditions are not met.");
			}
		}
	}
}
