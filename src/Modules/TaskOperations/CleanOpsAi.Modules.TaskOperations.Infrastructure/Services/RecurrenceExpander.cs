using CleanOpsAi.BuildingBlocks.Domain.Dtos;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services;

namespace CleanOpsAi.Modules.TaskOperations.Infrastructure.Services
{
	public class RecurrenceExpander : IRecurrenceExpander
	{
		public IReadOnlyList<DateTime> Expand(
		RecurrenceType type,
		RecurrenceConfig config,
		DateOnly from,
		DateOnly to)
		{
			var times = config.Times ?? [TimeOnly.MinValue];
			var results = new List<DateTime>();

			for (var date = from; date <= to; date = date.AddDays(1))
			{
				if (!MatchesType(type, config, date)) continue;

				foreach (var time in times)
					results.Add(date.ToDateTime(time));
			}

			return results;
		}

		private static bool MatchesType(
			RecurrenceType type, RecurrenceConfig config, DateOnly date) =>
			type switch
			{
				RecurrenceType.Daily => true,

				RecurrenceType.Weekly =>
					config.DaysOfWeek?.Contains(date.DayOfWeek) ?? false,

				RecurrenceType.Monthly =>
					config.DaysOfMonth?.Contains(date.Day) ?? false,

				RecurrenceType.Yearly =>
					config.MonthDays?.Any(m => m.Month == date.Month && m.Day == date.Day)
					?? false,

				_ => false
			};
	}
}
