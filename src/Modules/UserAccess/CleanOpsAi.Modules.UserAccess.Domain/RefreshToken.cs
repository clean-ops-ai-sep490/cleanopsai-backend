using CleanOpsAi.BuildingBlocks.Domain;

namespace CleanOpsAi.Modules.UserAccess.Domain
{
	public class RefreshToken : BaseEntity
	{
		public string Token { get; set; } = string.Empty;
		public Guid UserId { get; set; }
		public ApplicationUser User { get; set; } = null!;
		public DateTime ExpiresAt { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public bool IsRevoked { get; set; }
	}
}
