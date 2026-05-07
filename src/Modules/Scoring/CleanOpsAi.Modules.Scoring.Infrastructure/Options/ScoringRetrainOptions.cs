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
		public int MinApprovedAnnotations { get; set; } = 100;
		public int MaxSamplesPerBatch { get; set; } = 500;
		public bool AutoTriggerEnabled { get; set; } = true;
		public int AutoBatchThreshold { get; set; } = 100;

		public string TrainerCommand { get; set; } = string.Empty;
		public string? TrainerWorkingDirectory { get; set; }
		public int TrainerTimeoutSeconds { get; set; } = 7200;
		public bool RemoteTrainerEnabled { get; set; }
		public string? RemoteTrainerBaseUrl { get; set; }
		public string? RemoteTrainerApiKey { get; set; }
		public int RemoteTrainerTimeoutSeconds { get; set; } = 7200;
		public int RemoteTrainerPollIntervalSeconds { get; set; } = 5;
		public string RemoteTrainerCreatePath { get; set; } = "/retrain/jobs";
		public string RemoteTrainerStatusPathTemplate { get; set; } = "/retrain/jobs/{jobId}";

		public bool ObjectStorageEnabled { get; set; }
		public string ObjectStorageConnectionString { get; set; } = string.Empty;
		public string ObjectStorageModelsContainer { get; set; } = "models";
		public string ObjectStorageRetrainContainer { get; set; } = "retrain";
		public bool ReviewedSnapshotsEnabled { get; set; } = true;
		public string ReviewedSamplesContainer { get; set; } = "retrain-samples";
		public string ReviewedSamplesPrefix { get; set; } = "scoring/retrain-samples";
		public bool ReviewedBridgeEnabled { get; set; }
		public string? ReviewedBridgeCommand { get; set; }
		public bool UseExternalCandidateForRetrain { get; set; }

		public string CandidateYoloModelFile { get; set; } = "outputs/retrain/candidate/yolo_best.pt";
		public string CandidateUnetModelFile { get; set; } = "outputs/retrain/candidate/unet_best.pth";
		public string CandidateMetricsFile { get; set; } = "outputs/retrain/candidate_metrics.json";
		public string ActiveYoloObjectKey { get; set; } = "scoring/active/yolo/model.pt";
		public string ActiveUnetObjectKey { get; set; } = "scoring/active/unet/model.pth";
		public string ActiveMetricsObjectKey { get; set; } = "scoring/active/metrics/metrics.json";
		public string CandidatePrefix { get; set; } = "scoring/candidates";
		public string? ExternalCandidatePrefix { get; set; } = "scoring/external/latest";
		public string ManifestPrefix { get; set; } = "scoring/manifests";
		public string ArchivePrefix { get; set; } = "scoring/archive";

		public string YoloMapMetricKey { get; set; } = "yolo.map";
		public string UnetMiouMetricKey { get; set; } = "unet.miou";
		public double MinimumYoloMapImprovement { get; set; } = 0.005;
		public double MinimumUnetMiouImprovement { get; set; } = 0.005;
		public bool PromoteWhenNoBaseline { get; set; } = false;
		public string? PromotionCommand { get; set; }
		public string? RestartServiceCommand { get; set; }
	}
}
