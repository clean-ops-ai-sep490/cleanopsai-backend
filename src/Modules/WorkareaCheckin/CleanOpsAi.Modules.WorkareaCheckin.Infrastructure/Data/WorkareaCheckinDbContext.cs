using CleanOpsAi.Modules.WorkareaCheckin.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace CleanOpsAi.Modules.WorkareaCheckin.Infrastructure.Data
{
	public class WorkareaCheckinDbContext : DbContext
	{
		public DbSet<WorkareaCheckinPoint> WorkareaCheckinPoints { get; set; }
		public DbSet<AccessDevice> AccessDevices { get; set; }
		public DbSet<CheckinRecord> CheckinRecords { get; set; }

		public WorkareaCheckinDbContext()
		{
			
		}

		public WorkareaCheckinDbContext(DbContextOptions<WorkareaCheckinDbContext> options)
			: base(options)
		{

		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.HasDefaultSchema("workarea_checkin");

			base.OnModelCreating(modelBuilder);

			modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
		}
	}
}
