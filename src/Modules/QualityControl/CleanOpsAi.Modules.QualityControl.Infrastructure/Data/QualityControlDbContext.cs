using CleanOpsAi.Modules.QualityControl.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace CleanOpsAi.Modules.QualityControl.Infrastructure.Data
{
	public class QualityControlDbContext : DbContext
	{
		public DbSet<FcmToken> FcmTokens { get; set; }
		public DbSet<AppNotification> AppNotifications { get; set; }
		public DbSet<NotificationRecipient> NotificationRecipients { get; set; }

		public QualityControlDbContext()
		{
			
		}

		public QualityControlDbContext(DbContextOptions<QualityControlDbContext> options)
			: base(options)
		{

		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.HasDefaultSchema("quality_control");

			base.OnModelCreating(modelBuilder);

			modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
		}
	}
}
