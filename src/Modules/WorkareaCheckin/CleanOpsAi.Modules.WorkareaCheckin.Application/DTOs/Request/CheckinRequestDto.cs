namespace CleanOpsAi.Modules.WorkareaCheckin.Application.DTOs.Request
{
	public class CheckinRequestDto
	{
		public Guid WorkerId { get; set; }

		public Guid? WorkareaId { get; set; }

		public string? DeviceUuid { get; set; }

		public Guid? TaskId { get; set; }

		public Guid? TaskStepId { get; set; }

		public string? Notes { get; set; }
	}
}
