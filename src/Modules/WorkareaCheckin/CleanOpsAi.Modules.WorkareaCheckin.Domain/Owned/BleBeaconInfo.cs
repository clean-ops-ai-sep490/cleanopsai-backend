namespace CleanOpsAi.Modules.WorkareaCheckin.Domain.Owned
{ 
	public class BleBeaconInfo
	{
		public int? BatteryLevel { get; set; }
		public DateTime? LastSeenAt { get; set; }
		public DateTime? LastMaintenanceAt { get; set; }
	}
}
