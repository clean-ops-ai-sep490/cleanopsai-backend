using CleanOpsAi.Modules.Scoring.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanOpsAi.Modules.Scoring.Infrastructure.Data.Configurations
{
	public class ScoringJobConfiguration : IEntityTypeConfiguration<ScoringJob>
	{
		public void Configure(EntityTypeBuilder<ScoringJob> builder)
		{
			builder.HasKey(x => x.Id);

			builder.Property(x => x.RequestId)
				.IsRequired()
				.HasMaxLength(100);

			builder.Property(x => x.EnvironmentKey)
				.IsRequired()
				.HasMaxLength(50);

			builder.Property(x => x.FailureReason)
				.HasMaxLength(2000);

			builder.HasIndex(x => x.RequestId)
				.IsUnique();

			builder.HasIndex(x => x.Status);
			builder.HasIndex(x => x.Created);
			builder.HasQueryFilter(x => !x.IsDeleted);

			builder.HasMany(x => x.Results)
				.WithOne(x => x.ScoringJob)
				.HasForeignKey(x => x.ScoringJobId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
