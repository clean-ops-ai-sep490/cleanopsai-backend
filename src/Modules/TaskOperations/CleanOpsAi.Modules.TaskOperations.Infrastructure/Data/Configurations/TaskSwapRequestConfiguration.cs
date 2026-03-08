using CleanOpsAi.Modules.TaskOperations.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders; 

namespace CleanOpsAi.Modules.TaskOperations.Infrastructure.Data.Configurations
{
	public class TaskSwapRequestConfiguration : IEntityTypeConfiguration<TaskSwapRequest>
	{
		public void Configure(EntityTypeBuilder<TaskSwapRequest> builder)
		{
			builder.HasKey(x => x.Id);

			builder.Property(x => x.Status)
				.IsRequired();

			builder.Property(x => x.ReviewedBy)
				.HasMaxLength(450);

			builder.HasOne(x => x.TaskAssignment)
				.WithMany(x => x.TaskSwapRequests)
				.HasForeignKey(x => x.TaskAssignmentId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
