namespace CleanOpsAi.Modules.WorkareaCheckin.Application.DTOs.Response
{
	public class WorkareaCheckinPointDto
	{
		public Guid Id { get; set; }

		public Guid WorkareaId { get; set; }

		public string Name { get; set; } = null!;

		public string Code { get; set; } = null!;
	}
}
