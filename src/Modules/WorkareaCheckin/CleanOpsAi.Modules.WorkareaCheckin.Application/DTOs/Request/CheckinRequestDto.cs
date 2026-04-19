namespace CleanOpsAi.Modules.WorkareaCheckin.Application.DTOs.Request
{
	public class CheckinRequestDto
	{
		public Guid? CheckinPointId { get; set; }

		public string? Code { get; set; }  

		public Guid WorkerId { get; set; }

		public Guid? WorkareaId { get; set; } // BLE

		public string? DeviceUuid { get; set; }

		public int? Rssi { get; set; }

		public Guid? TaskId { get; set; }

		public Guid? TaskStepId { get; set; }

		public string? Notes { get; set; }
	}
}
