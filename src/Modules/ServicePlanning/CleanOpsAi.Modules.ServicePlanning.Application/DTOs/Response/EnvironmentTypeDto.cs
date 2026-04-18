namespace CleanOpsAi.Modules.ServicePlanning.Application.DTOs.Response
{
	public class EnvironmentTypeDto
	{
		public Guid Id { get; set; }

		public string Name { get; set; } = null!;

		public string? Description { get; set; }
	}
}
