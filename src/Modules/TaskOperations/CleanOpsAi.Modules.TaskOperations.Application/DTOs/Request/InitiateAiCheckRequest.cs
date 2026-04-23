using System.ComponentModel.DataAnnotations; 

namespace CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request
{
	public class InitiateAiCheckRequest
	{
		[Required]
		public Guid TaskStepExecutionId { get; set; }
	}
}
