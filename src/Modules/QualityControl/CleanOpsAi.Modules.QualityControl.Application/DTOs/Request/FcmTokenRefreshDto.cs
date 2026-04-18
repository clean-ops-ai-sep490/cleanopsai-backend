namespace CleanOpsAi.Modules.QualityControl.Application.DTOs.Request
{
	public class FcmTokenRefreshDto
	{
		public string Token { get; set; } = null!;
		public string UniqueId { get; set; } = null!;
	}
}
