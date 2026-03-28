using System.Text.Json.Serialization;

namespace CleanOpsAi.BuildingBlocks.Domain.Dtos
{
	public class RecurrenceConfig
	{ 
		[JsonPropertyName("times")] 
		public List<TimeOnly>? Times { get; set; }
		
		[JsonPropertyName("daysOfWeek")] 
		public List<DayOfWeek>? DaysOfWeek { get; set; } 

		[JsonPropertyName("daysOfMonth")] 
		public List<int>? DaysOfMonth { get; set; } 
		
		[JsonPropertyName("monthDays")] 
		public List<MonthDay>? MonthDays { get; set; } 
	}

	public record MonthDay(int Month, int Day);
}
