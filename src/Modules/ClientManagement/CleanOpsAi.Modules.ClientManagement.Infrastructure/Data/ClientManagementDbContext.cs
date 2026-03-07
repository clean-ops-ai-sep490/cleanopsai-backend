using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace CleanOpsAi.Modules.ClientManagement.Infrastructure.Data
{
	public class ClientManagementDbContext : DbContext
	{
		public ClientManagementDbContext()
		{
			
		}

		public ClientManagementDbContext(DbContextOptions<ClientManagementDbContext> options) : base(options) 
		{
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
		}
	}
}
