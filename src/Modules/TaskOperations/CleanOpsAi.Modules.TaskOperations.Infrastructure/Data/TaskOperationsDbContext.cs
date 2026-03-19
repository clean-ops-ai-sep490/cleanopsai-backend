
using CleanOpsAi.Modules.TaskOperations.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace CleanOpsAi.Modules.TaskOperations.Infrastructure.Data
{
	public class TaskOperationsDbContext : DbContext
	{
		public DbSet<TaskAssignment> TaskAssignments { get; set; }

		public TaskOperationsDbContext()
		{
			
		}

		public TaskOperationsDbContext(DbContextOptions<TaskOperationsDbContext> options)
			: base(options)
		{
			
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.HasDefaultSchema("task_operations");

			base.OnModelCreating(modelBuilder);

			modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
		}
	}
}
