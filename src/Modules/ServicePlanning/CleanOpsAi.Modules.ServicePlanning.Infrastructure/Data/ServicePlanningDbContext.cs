using CleanOpsAi.Modules.ServicePlanning.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace CleanOpsAi.Modules.ServicePlanning.Infrastructure.Data
{
	public class ServicePlanningDbContext : DbContext
	{
		public DbSet<Step> Steps { get; set; }
		public DbSet<Sop> Sops { get; set; }
		public DbSet<SopStep> SopSteps { get; set; }
		public DbSet<TaskSchedule> TaskSchedules { get; set; }

		public ServicePlanningDbContext()
		{
			
		}

		public ServicePlanningDbContext(DbContextOptions<ServicePlanningDbContext> options)
			: base(options)
		{
			
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
		}
	}
}
