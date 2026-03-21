using CleanOpsAi.Modules.TaskOperations.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders; 

namespace CleanOpsAi.Modules.TaskOperations.Infrastructure.Data.Configurations
{
	public class TaskAssignmentConfiguration : IEntityTypeConfiguration<TaskAssignment>
	{
		public void Configure(EntityTypeBuilder<TaskAssignment> builder)
		{
			builder.HasKey(x => x.Id);

			builder.Property(x => x.Status)
				.IsRequired();

			builder.HasIndex(x => new { x.TaskScheduleId, x.ScheduledStartAt })
				.IsUnique()
				.HasFilter("is_deleted = false");

			builder.Property(x => x.ScheduledStartAt)
				.IsRequired();

			builder.Property(x => x.IsAdhocTask)
				.IsRequired();

			builder.Property(x => x.NameAdhocTask)
				.HasMaxLength(500);

			builder.Property(x => x.DisplayLocation)
				.HasMaxLength(500);

			builder.HasIndex(x => x.TaskScheduleId);

			builder.HasMany(x => x.TaskStepExecutions)
				.WithOne(x => x.TaskAssignment)
				.HasForeignKey(x => x.TaskAssignmentId);

			builder.HasMany(x => x.TaskSwapRequests)
				.WithOne(x => x.TaskAssignment)
				.HasForeignKey(x => x.TaskAssignmentId);

			builder.HasMany(x => x.IssueReports)
				.WithOne(x => x.TaskAssignment)
				.HasForeignKey(x => x.TaskAssignmentId);

			builder.HasMany(x => x.EmergencyLeaveRequests)
				.WithOne(x => x.TaskAssignment)
				.HasForeignKey(x => x.TaskAssignmentId);

			builder.HasMany(x => x.EquipmentRequests)
				.WithOne()
				.HasForeignKey(x => x.TaskAssignmentId);

			builder.HasMany(x => x.AdHocRequests)
				.WithOne(x => x.TaskAssignment)
				.HasForeignKey(x => x.TaskAssignmentId);

			builder.HasIndex(x => x.AssigneeId);
			builder.HasIndex(x => x.TaskScheduleId);
			builder.HasIndex(x => x.Status);

			builder.HasQueryFilter(x => !x.IsDeleted);
		}
	}
}
