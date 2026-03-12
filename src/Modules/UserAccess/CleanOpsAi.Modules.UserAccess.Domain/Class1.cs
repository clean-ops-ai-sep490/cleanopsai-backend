using Microsoft.AspNetCore.Identity;

namespace CleanOpsAi.Modules.UserAccess.Domain
{
	public class ApplicationUser : IdentityUser<Guid>
	{
		public string FullName { get; set; } = string.Empty;
		public UserRole Role { get; set; } = UserRole.Worker;
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	}
}
