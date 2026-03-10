using System.Text.Json;

namespace CleanOpsAi.Modules.ServicePlanning.Application.DTOs.Request
{
	public class StepCreateDto
	{
		public string ActionKey { get; set; } = null!;

		public string Name { get; set; } = null!;

		public string? Description { get; set; }

		public JsonElement ConfigSchema { get; set; } 
	}
}
