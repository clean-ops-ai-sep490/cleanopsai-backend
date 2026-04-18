
using CleanOpsAi.Modules.TaskOperations.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace CleanOpsAi.Modules.TaskOperations.Infrastructure.Data
{
	public class TaskOperationsDbContext : DbContext
	{
		public DbSet<TaskAssignment> TaskAssignments { get; set; }
		public DbSet<TaskSwapRequest> TaskSwapRequests { get; set; }
		public DbSet<AdHocRequest> AdHocRequests { get; set; }
		public DbSet<ComplianceCheck> ComplianceChecks { get; set; }
		public DbSet<EmergencyLeaveRequest> EmergencyLeaveRequests { get; set; }
		public DbSet<EquipmentRequest> EquipmentRequests { get; set; }
		public DbSet<TaskHistory> TaskHistories { get; set; }
		public DbSet<TaskStepExecution> TaskStepExecutions { get; set; }
		public DbSet<TaskStepExecutionImage> TaskStepExecutionImages { get; set; }
		public DbSet<IssueReport> IssueReports { get; set; }

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
