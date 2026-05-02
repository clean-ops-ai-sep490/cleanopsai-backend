namespace CleanOpsAi.Modules.ServicePlanning.Application.DTOs.Response
{
	public class ScheduleByWorkAreaDto
	{
		public Guid Id { get; set; }
		public string Name { get; set; } = null!;
		public Guid WorkAreaId { get; set; }
		public string? DisplayLocation { get; set; }
		public Guid AssigneeId { get; set; }
		public string? AssigneeName { get; set; } 
	}
}
