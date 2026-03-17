namespace CleanOpsAi.Modules.ServicePlanning.Application.DTOs
{
	public class JobOptionsConfig
	{
		public const string SectionName = "Jobs:WeeklyTaskGeneration";
		public string CronExpression { get; set; } = "0 0 0 ? * SUN";
		public int LookAheadDays { get; set; } = 7;
		public bool Enabled { get; set; } = true;
	}
}
