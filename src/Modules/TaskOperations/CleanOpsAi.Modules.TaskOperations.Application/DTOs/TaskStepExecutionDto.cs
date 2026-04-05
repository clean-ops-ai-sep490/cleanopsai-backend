using CleanOpsAi.Modules.TaskOperations.Domain.Enums;
using System.Text.Json;

namespace CleanOpsAi.Modules.TaskOperations.Application.DTOs
{
	public class TaskStepExecutionDto
	{
		public Guid Id { get; set; }
		public Guid SopStepId { get; set; }
		public int StepOrder { get; set; }
		public TaskStepExecutionStatus Status { get; set; }
		public JsonElement ConfigSnapshot { get; set; }
		public JsonElement ResultData { get; set; }
		public Guid? NextStepId { get; set; }
	}
}
