using System.Text.Json;

namespace CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request
{
	public class SubmitStepExecutionDto
	{
		public Guid WorkerId { get; set; }
		public JsonElement ResultData { get; set; } 
	}
}
