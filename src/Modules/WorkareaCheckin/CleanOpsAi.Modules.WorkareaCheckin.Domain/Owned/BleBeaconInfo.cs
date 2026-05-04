namespace CleanOpsAi.Modules.WorkareaCheckin.Domain.Owned
{
	public class BleBeaconInfo
	{
		// ── Identification  
		public string? ServiceUuid { get; set; }

		// ── Signal 
		public int? TxPower { get; set; }             // +3 dBm (từ beacon broadcast)
		public int? RssiThreshold { get; set; }       // -85 → dưới mức này bỏ qua
		public int? LastRssi { get; set; }          

		// ── Status  
		public int? BatteryLevel { get; set; }        // % pin
		public DateTime? LastSeenAt { get; set; }     // lần cuối scan thấy
		public DateTime? LastMaintenanceAt { get; set; }
	}
}