using CleanOpsAi.BuildingBlocks.Domain;
using CleanOpsAi.Modules.QualityControl.Domain.Enums;

namespace CleanOpsAi.Modules.QualityControl.Domain.Entities
{
	public class FcmToken : BaseAuditableEntity
	{
		public Guid UserId { get; set; }

		public Guid? WorkerId { get; set; }

		public string Token { get; set; } = null!;

		public string UniqueId { get; set; } = null!;

		public DevicePlatform Platform { get; set; }  

		public string? DeviceName { get; set; }

		public bool IsActive { get; set; }

		public DateTime LastUsed { get; set; } = DateTime.UtcNow;
	}
}
