using CleanOpsAi.Modules.Scoring.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanOpsAi.Modules.Scoring.Infrastructure.Data.Configurations
{
	public class ScoringRetrainBatchConfiguration : IEntityTypeConfiguration<ScoringRetrainBatch>
	{
		public void Configure(EntityTypeBuilder<ScoringRetrainBatch> builder)
		{
			builder.HasKey(x => x.Id);

			builder.Property(x => x.FailureReason)
				.HasMaxLength(2000);

			builder.Property(x => x.MetricKey)
				.HasMaxLength(200);

			builder.Property(x => x.PromotionReason)
				.HasMaxLength(2000);

			builder.HasIndex(x => x.Status);
			builder.HasIndex(x => x.RequestedAtUtc);
			builder.HasIndex(x => x.Created);
			builder.HasQueryFilter(x => !x.IsDeleted);

			builder.HasMany(x => x.Runs)
				.WithOne(x => x.Batch)
				.HasForeignKey(x => x.ScoringRetrainBatchId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
