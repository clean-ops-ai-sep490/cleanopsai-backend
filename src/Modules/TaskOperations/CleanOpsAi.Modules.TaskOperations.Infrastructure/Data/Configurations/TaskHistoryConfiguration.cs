using CleanOpsAi.Modules.TaskOperations.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders; 

namespace CleanOpsAi.Modules.TaskOperations.Infrastructure.Data.Configurations
{
	public class TaskHistoryConfiguration : IEntityTypeConfiguration<TaskHistory>
	{
		public void Configure(EntityTypeBuilder<TaskHistory> builder)
		{
			builder.HasKey(x => x.Id);

			builder.Property(x => x.Metadata)
				.HasColumnType("jsonb");

			builder.Property(x => x.Status)
				.IsRequired(); 
			 
			builder.HasOne(x => x.TaskAssignment)
				.WithMany()
				.HasForeignKey(x => x.TaskAssignmentId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
