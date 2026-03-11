using CleanOpsAi.Modules.UserAccess.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CleanOpsAi.Modules.UserAccess.Infrastructure.Persistence
{
	public class UserAccessDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
	{
		public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

		public UserAccessDbContext(DbContextOptions<UserAccessDbContext> options)
			: base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);

			builder.HasDefaultSchema("user_access");

			// ApplicationUser configuration
			builder.Entity<ApplicationUser>(entity =>
			{
				entity.Property(e => e.Role)
					.HasConversion<int>()
					.HasDefaultValue(UserRole.Worker)
					.HasSentinel((UserRole)0);
			});

			// RefreshToken configuration
			builder.Entity<RefreshToken>(entity =>
			{
				entity.HasIndex(e => e.Token).IsUnique();
				entity.HasOne(e => e.User)
					.WithMany()
					.HasForeignKey(e => e.UserId)
					.OnDelete(DeleteBehavior.Cascade);
			});

			// Seed the 5 roles
			builder.Entity<IdentityRole<Guid>>().HasData(
				new IdentityRole<Guid> { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = nameof(UserRole.Worker), NormalizedName = nameof(UserRole.Worker).ToUpperInvariant(), ConcurrencyStamp = "11111111-1111-1111-1111-111111111111" },
				new IdentityRole<Guid> { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = nameof(UserRole.Admin), NormalizedName = nameof(UserRole.Admin).ToUpperInvariant(), ConcurrencyStamp = "22222222-2222-2222-2222-222222222222" },
				new IdentityRole<Guid> { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Name = nameof(UserRole.Manager), NormalizedName = nameof(UserRole.Manager).ToUpperInvariant(), ConcurrencyStamp = "33333333-3333-3333-3333-333333333333" },
				new IdentityRole<Guid> { Id = Guid.Parse("44444444-4444-4444-4444-444444444444"), Name = nameof(UserRole.Supervisor), NormalizedName = nameof(UserRole.Supervisor).ToUpperInvariant(), ConcurrencyStamp = "44444444-4444-4444-4444-444444444444" },
				new IdentityRole<Guid> { Id = Guid.Parse("55555555-5555-5555-5555-555555555555"), Name = nameof(UserRole.Supporter), NormalizedName = nameof(UserRole.Supporter).ToUpperInvariant(), ConcurrencyStamp = "55555555-5555-5555-5555-555555555555" }
			);
		}
	}
}
