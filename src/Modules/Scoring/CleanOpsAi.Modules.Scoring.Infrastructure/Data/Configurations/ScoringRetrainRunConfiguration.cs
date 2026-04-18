using CleanOpsAi.Modules.Scoring.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanOpsAi.Modules.Scoring.Infrastructure.Data.Configurations
{
	public class ScoringRetrainRunConfiguration : IEntityTypeConfiguration<ScoringRetrainRun>
	{
		public void Configure(EntityTypeBuilder<ScoringRetrainRun> builder)
		{
			builder.HasKey(x => x.Id);

			builder.Property(x => x.Mode)
				.IsRequired()
				.HasMaxLength(50);

			builder.Property(x => x.Message)
				.HasMaxLength(4000);

			builder.HasIndex(x => x.ScoringRetrainBatchId);
			builder.HasIndex(x => x.Status);
			builder.HasIndex(x => x.StartedAtUtc);
			builder.HasQueryFilter(x => !x.IsDeleted);
		}
	}
}
