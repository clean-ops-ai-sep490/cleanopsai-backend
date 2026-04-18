using CleanOpsAi.Modules.WorkareaCheckin.Application.DTOs.Request;
using CleanOpsAi.Modules.WorkareaCheckin.Domain.Enums; 

namespace CleanOpsAi.Modules.WorkareaCheckin.Application.DTOs.Response
{
	public class AccessDeviceDto
	{
		public Guid Id { get; set; }
		public Guid WorkareaCheckinPointId { get; set; }
		public DeviceType DeviceType { get; set; }
		public string? Name { get; set; }
		public string? Code { get; set; }
		public string? Identifier { get; set; }
		public string? InstallationLocation { get; set; }

		public DeviceStatus Status { get; set; } 
		public BleBeaconInfoDto? BleInfo { get; set; } 
	}
}
