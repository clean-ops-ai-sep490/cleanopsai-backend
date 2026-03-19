using Microsoft.EntityFrameworkCore; 
using System.Reflection;

namespace CleanOpsAi.Modules.Workforce.Infrastructure.Data
{
	public class WorkforceDbContext : DbContext
	{
		public WorkforceDbContext()
		{

		}

		public WorkforceDbContext(DbContextOptions<WorkforceDbContext> options)
			: base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.HasDefaultSchema("workforce");

			base.OnModelCreating(modelBuilder);
			 
			modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
		}

		  
	}
}
