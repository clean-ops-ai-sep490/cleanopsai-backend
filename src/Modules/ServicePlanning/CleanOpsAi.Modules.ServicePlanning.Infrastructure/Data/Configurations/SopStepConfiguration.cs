using CleanOpsAi.Modules.ServicePlanning.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanOpsAi.Modules.ServicePlanning.Infrastructure.Data.Configurations
{
	public class SopStepConfiguration : IEntityTypeConfiguration<SopStep>
	{
		public void Configure(EntityTypeBuilder<SopStep> builder)
		{
			builder.HasKey(x => x.Id);

			builder.Property(x => x.StepOrder)
				.IsRequired();

			builder.Property(x => x.ConfigDetail)
				.IsRequired()
				.HasColumnType("jsonb");  

			builder.HasOne(x => x.Sop)
				.WithMany(x => x.SopSteps)
				.HasForeignKey(x => x.SopId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasOne(x => x.Step)
				.WithMany(x => x.SopSteps)
				.HasForeignKey(x => x.StepId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasIndex(x => new { x.SopId, x.StepOrder })
				.IsUnique().HasFilter("is_deleted = false");

			builder.HasQueryFilter(x => !x.IsDeleted);
		}
	}
}
