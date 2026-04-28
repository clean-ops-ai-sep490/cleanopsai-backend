namespace CleanOpsAi.Modules.WorkareaCheckin.Application.DTOs.Request
{
	public class WorkareaCheckinPointCreateDto
	{
		public Guid WorkareaId { get; set; } 

		public string Name { get; set; } = null!; 
	}
}
