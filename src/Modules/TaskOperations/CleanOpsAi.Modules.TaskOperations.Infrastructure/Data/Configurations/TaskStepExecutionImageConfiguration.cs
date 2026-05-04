using CleanOpsAi.Modules.TaskOperations.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders; 

namespace CleanOpsAi.Modules.TaskOperations.Infrastructure.Data.Configurations
{
	public class TaskStepExecutionImageConfiguration : IEntityTypeConfiguration<TaskStepExecutionImage>
	{
		public void Configure(EntityTypeBuilder<TaskStepExecutionImage> builder)
		{
			builder.HasKey(x => x.Id);

			builder.Property(x => x.ImageUrl)
				.IsRequired()
				.HasMaxLength(1000);

			builder.Property(x => x.ImageType)
				.IsRequired();

			builder.HasOne(x => x.TaskStepExecution)
				.WithMany(x => x.TaskStepExecutionImages)
				.HasForeignKey(x => x.TaskStepExecutionId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.Property(x => x.QualityScore).HasPrecision(5, 2);  
			builder.Property(x => x.Verdict).HasMaxLength(500);

			builder.HasQueryFilter(x => !x.IsDeleted);
		}
	}
}
