using CleanOpsAi.Modules.Scoring.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.Scoring.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.Scoring.Application.Services;
using CleanOpsAi.Modules.Scoring.Infrastructure.Consumers;
using CleanOpsAi.Modules.Scoring.Infrastructure.Data;
using CleanOpsAi.Modules.Scoring.Infrastructure.Jobs;
using CleanOpsAi.Modules.Scoring.Infrastructure.Options;
using CleanOpsAi.Modules.Scoring.Infrastructure.Repositories;
using CleanOpsAi.Modules.Scoring.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;

namespace Microsoft.Extensions.DependencyInjection;
public static class DependencyInjection
{
	public static void InfrastructureScoringModule(this IHostApplicationBuilder builder)
	{
		builder.Services.AddDbContext<ScoringDbContext>(options =>
		{
			options.UseNpgsql(builder.Configuration["ConnectionStrings:DefaultConnection"])
				.UseSnakeCaseNamingConvention()
				.EnableSensitiveDataLogging()
				.EnableDetailedErrors();
			options.LogTo(Console.WriteLine, new[] { DbLoggerCategory.Database.Command.Name }, LogLevel.Information);
		});

		builder.Services.Configure<ScoringServiceOptions>(
			builder.Configuration.GetSection("ScoringService"));

		builder.Services.Configure<ScoringRetrainOptions>(
			builder.Configuration.GetSection(ScoringRetrainOptions.SectionName));

		var retrainOptions = builder.Configuration
			.GetSection(ScoringRetrainOptions.SectionName)
			.Get<ScoringRetrainOptions>() ?? new ScoringRetrainOptions();

		if (retrainOptions.WeeklyJobEnabled)
		{
			builder.Services.AddQuartz(q =>
			{
				q.AddJob<WeeklyScoringRetrainJob>(j =>
					j.WithIdentity("weekly-scoring-retrain", "scoring"));

				q.AddTrigger(t => t
					.ForJob("weekly-scoring-retrain", "scoring")
					.WithIdentity("weekly-scoring-retrain-trigger", "scoring")
					.WithCronSchedule(
						retrainOptions.WeeklyCronExpression,
						x => x.InTimeZone(TimeZoneInfo.FindSystemTimeZoneById(retrainOptions.TimeZoneId))));
			});

			builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
		}

		builder.Services.AddHttpClient<IScoringInferenceClient, ScoringInferenceClient>((sp, client) =>
		{
			var options = sp.GetRequiredService<IOptions<ScoringServiceOptions>>().Value;
			if (!string.IsNullOrWhiteSpace(options.BaseUrl))
			{
				client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
			}
			client.Timeout = TimeSpan.FromSeconds(Math.Max(30, options.TimeoutSeconds));
		});

		builder.Services.AddScoped<IScoringJobRepository, ScoringJobRepository>();
		builder.Services.AddScoped<ISupervisorManagedWorkerQueryService, SupervisorManagedWorkerQueryService>();
		builder.Services.AddScoped<IWorkerLookupQueryService, WorkerLookupQueryService>();
		builder.Services.AddScoped<IScoringAnnotationArtifactService, ScoringAnnotationArtifactService>();
		builder.Services.AddScoped<IScoringRetrainRequestHandler, ScoringRetrainRequestedConsumer>();
		builder.Services.AddScoped<IScoringJobService, ScoringJobService>();
	}
}
