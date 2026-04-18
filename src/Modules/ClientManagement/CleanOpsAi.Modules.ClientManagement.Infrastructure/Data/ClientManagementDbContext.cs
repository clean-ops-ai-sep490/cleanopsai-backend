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
			modelBuilder.HasDefaultSchema("client_management");

			base.OnModelCreating(modelBuilder);

			modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
		}
	}
}
