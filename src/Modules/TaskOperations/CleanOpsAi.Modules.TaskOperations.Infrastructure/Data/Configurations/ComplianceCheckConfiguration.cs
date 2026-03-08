using CleanOpsAi.Modules.TaskOperations.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders; 

namespace CleanOpsAi.Modules.TaskOperations.Infrastructure.Data.Configurations
{
	public class ComplianceCheckConfiguration : IEntityTypeConfiguration<ComplianceCheck>
	{
		public void Configure(EntityTypeBuilder<ComplianceCheck> builder)
		{
			builder.HasKey(x => x.Id);

			builder.Property(x => x.Status)
				.IsRequired();

			builder.Property(x => x.Type)
				.IsRequired();

			builder.Property(x => x.Feedback)
				.HasMaxLength(1000);

			builder.HasOne(x => x.TaskStepExecution)
				.WithMany(x => x.ComplianceChecks)
				.HasForeignKey(x => x.TaskStepExecutionId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
