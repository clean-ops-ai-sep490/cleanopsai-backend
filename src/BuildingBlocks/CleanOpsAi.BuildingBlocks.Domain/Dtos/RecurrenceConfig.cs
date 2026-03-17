namespace CleanOpsAi.BuildingBlocks.Domain.Dtos
{
	public class RecurrenceConfig
	{
		// DAILY: chạy lúc mấy giờ trong ngày
		// type = Daily  →  Times bắt buộc
		public List<TimeOnly>? Times { get; set; }

		// WEEKLY: thứ mấy + lúc mấy giờ
		// type = Weekly  →  DaysOfWeek + Times bắt buộc
		public List<DayOfWeek>? DaysOfWeek { get; set; }

		// MONTHLY: ngày mấy trong tháng + lúc mấy giờ
		// type = Monthly  →  DaysOfMonth + Times bắt buộc
		public List<int>? DaysOfMonth { get; set; }

		// YEARLY: tháng + ngày + lúc mấy giờ
		// type = Yearly  →  MonthDays + Times bắt buộc
		public List<MonthDay>? MonthDays { get; set; }
	}

	public record MonthDay(int Month, int Day);
}
