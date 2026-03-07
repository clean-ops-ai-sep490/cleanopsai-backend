using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace CleanOpsAi.Modules.ServicePlanning.Infrastructure.Data
{
	public class ServicePlanningDbContext : DbContext
	{
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
