namespace CleanOpsAi.Modules.Scoring.Infrastructure.Options
{
	public class ScoringRetrainOptions
	{
		public const string SectionName = "ScoringRetrain";

		public bool WeeklyJobEnabled { get; set; }
		public string WeeklyCronExpression { get; set; } = "0 0 2 ? * SUN *";
		public string TimeZoneId { get; set; } = "SE Asia Standard Time";
		public int LookbackDays { get; set; } = 7;
		public int MinReviewedSamples { get; set; } = 25;
		public int MaxSamplesPerBatch { get; set; } = 500;

		public string TrainerCommand { get; set; } = "docker compose --profile trainer run --rm cleanops-ai-scoring-trainer";
		public string? TrainerWorkingDirectory { get; set; }
		public int TrainerTimeoutSeconds { get; set; } = 7200;

		public bool ObjectStorageEnabled { get; set; }
		public string ObjectStorageServiceUrl { get; set; } = "http://host.docker.internal:9000";
		public string ObjectStorageAccessKey { get; set; } = "admin";
		public string ObjectStorageSecretKey { get; set; } = "password123";
		public bool ObjectStorageUseSsl { get; set; }
		public bool ObjectStorageForcePathStyle { get; set; } = true;
		public string ObjectStorageBucket { get; set; } = "ai-scoring-models";

		public string CandidateYoloModelFile { get; set; } = "outputs/retrain/candidate/yolo_best.pt";
		public string CandidateUnetModelFile { get; set; } = "outputs/retrain/candidate/unet_best.pth";
		public string CandidateMetricsFile { get; set; } = "outputs/retrain/candidate_metrics.json";
		public string ActiveYoloObjectKey { get; set; } = "active/yolo/model.pt";
		public string ActiveUnetObjectKey { get; set; } = "active/unet/model.pth";
		public string ActiveMetricsObjectKey { get; set; } = "active/metrics/metrics.json";
		public string CandidatePrefix { get; set; } = "candidates";
		public string? ExternalCandidatePrefix { get; set; }
		public string ManifestPrefix { get; set; } = "manifests";
		public string ArchivePrefix { get; set; } = "archive";

		public string YoloMapMetricKey { get; set; } = "yolo.map";
		public string UnetMiouMetricKey { get; set; } = "unet.miou";
		public double MinimumYoloMapImprovement { get; set; } = 0.0;
		public double MinimumUnetMiouImprovement { get; set; } = 0.0;
		public bool PromoteWhenNoBaseline { get; set; } = false;
		public string? PromotionCommand { get; set; }
		public string? RestartServiceCommand { get; set; }
	}
}
