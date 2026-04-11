namespace CleanOpsAi.Modules.WorkareaCheckin.Application.DTOs.Request
{
	public class WorkareaCheckinPointUpdateDto
	{
		public Guid WorkareaId { get; set; }

		public string Name { get; set; } = null!;

		public string Code { get; set; } = null!;
	}
}
