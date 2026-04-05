using CleanOpsAi.Modules.TaskOperations.Domain.Enums;
using System.Text.Json;

namespace CleanOpsAi.Modules.TaskOperations.Application.DTOs.Response
{
	public class TaskStepExecutionDetailDto
	{
		public Guid Id { get; set; }
		public Guid SopStepId { get; set; }
		public int StepOrder { get; set; }
		public TaskStepExecutionStatus Status { get; set; } 
		public JsonElement ConfigSnapshot { get; set; }
		public JsonElement ResultData { get; set; }
	}
}
