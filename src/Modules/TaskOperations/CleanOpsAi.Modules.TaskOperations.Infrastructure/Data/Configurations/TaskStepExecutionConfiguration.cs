using CleanOpsAi.Modules.TaskOperations.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders; 

namespace CleanOpsAi.Modules.TaskOperations.Infrastructure.Data.Configurations
{
	public class TaskStepExecutionConfiguration : IEntityTypeConfiguration<TaskStepExecution>
	{
		public void Configure(EntityTypeBuilder<TaskStepExecution> builder)
		{
			builder.HasKey(x => x.Id); 

			builder.Property(x => x.ResultData)
				.HasColumnType("jsonb"); 

			builder.HasOne(x => x.TaskAssignment)
				.WithMany(x => x.TaskStepExecutions)
				.HasForeignKey(x => x.TaskAssignmentId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasIndex(x => x.TaskAssignmentId);

			builder.HasQueryFilter(x => !x.IsDeleted);

			builder.Property(x => x.StepOrder);
		}
	}
}
