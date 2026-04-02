using CleanOpsAi.Modules.QualityControl.Domain.Enums;

namespace CleanOpsAi.Modules.QualityControl.Application.DTOs.Request
{
	public class FcmTokenCreateDto
	{
		public string Token { get; set; } = null!;
		public string UniqueId { get; set; } = null!;
		public DevicePlatform Platform { get; set; }
		public string? DeviceName { get; set; }

		public Guid? WorkerId { get; set; }
	}
}
