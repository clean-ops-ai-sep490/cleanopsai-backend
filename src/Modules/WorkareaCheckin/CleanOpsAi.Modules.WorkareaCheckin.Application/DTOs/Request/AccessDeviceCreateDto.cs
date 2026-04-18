using CleanOpsAi.Modules.WorkareaCheckin.Domain.Enums; 
using System.ComponentModel.DataAnnotations; 

namespace CleanOpsAi.Modules.WorkareaCheckin.Application.DTOs.Request
{
	public class AccessDeviceCreateDto
	{
		public Guid WorkareaCheckinPointId { get; set; }

		[Required]
		public DeviceType DeviceType { get; set; }

		[MaxLength(100)]
		public string? Name { get; set; }

		public string? Code { get; set; }

		[MaxLength(200)]
		public string? Identifier { get; set; }

		[MaxLength(200)]
		public string? InstallationLocation { get; set; }

		public BleBeaconInfoDto? BleInfo { get; set; }

	}

	public class BleBeaconInfoDto
	{
		public string? ServiceUuid { get; set; }
		public int? TxPower { get; set; }
		public int? RssiThreshold { get; set; }
	}
}
