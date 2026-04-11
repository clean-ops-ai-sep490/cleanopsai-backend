using System.ComponentModel.DataAnnotations; 

namespace CleanOpsAi.Modules.WorkareaCheckin.Domain.Owned
{
	public class DevicePosition
	{
		public decimal? X { get; set; }
		public decimal? Y { get; set; }
		public decimal? Z { get; set; }
		public int? FloorLevel { get; set; }

		[MaxLength(255)]
		public string? InstallationLocation { get; set; }
	}
}
