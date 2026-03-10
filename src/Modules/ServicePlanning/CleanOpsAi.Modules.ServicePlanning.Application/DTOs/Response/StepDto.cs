using System.Text.Json;

namespace CleanOpsAi.Modules.ServicePlanning.Application.DTOs.Response
{
	public class StepDto
	{
		public Guid Id { get; set; }

		public string ActionKey { get; set; } = null!;

		public string Name { get; set; } = null!;

		public string? Description { get; set; }

		public JsonElement ConfigSchema { get; set; }  
	}
}
