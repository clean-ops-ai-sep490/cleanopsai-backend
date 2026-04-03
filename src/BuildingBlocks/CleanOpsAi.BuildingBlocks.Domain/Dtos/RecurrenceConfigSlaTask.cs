using System.Text.Json.Serialization;

namespace CleanOpsAi.BuildingBlocks.Domain.Dtos
{
	public class RecurrenceConfigSlaTask
	{
		[JsonPropertyName("interval")]
		public int Interval { get; set; } = 1;

		[JsonPropertyName("daysOfWeek")]
		public List<DayOfWeek>? DaysOfWeek { get; set; }

		[JsonPropertyName("daysOfMonth")]
		public List<int>? DaysOfMonth { get; set; }

		[JsonPropertyName("monthDays")]
		public List<MonthDaySlaTask>? MonthDays { get; set; }
	}
	public record MonthDaySlaTask(int Month, int Day);

}
