namespace CleanOpsAi.Modules.ServicePlanning.Application.DTOs.Request
{
	public class GetsTaskScheduleQuery
	{
		public string? Name { get; set; }

		public bool IsDescending { get; set; } = false;
	}
}
