using System.Text.Json;

namespace CleanOpsAi.Modules.ServicePlanning.Application.DTOs.Request
{
	public class StepUpdateDto
	{
		public string? Name { get; set; }

		public string? Description { get; set; }

		public JsonElement? ConfigSchema { get; set; }
	}
}
