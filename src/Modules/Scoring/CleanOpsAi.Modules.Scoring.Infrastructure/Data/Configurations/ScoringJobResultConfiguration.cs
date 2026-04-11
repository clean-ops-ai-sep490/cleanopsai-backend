using CleanOpsAi.Modules.Scoring.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanOpsAi.Modules.Scoring.Infrastructure.Data.Configurations
{
	public class ScoringJobResultConfiguration : IEntityTypeConfiguration<ScoringJobResult>
	{
		public void Configure(EntityTypeBuilder<ScoringJobResult> builder)
		{
			builder.HasKey(x => x.Id);

			builder.Property(x => x.SourceType)
				.IsRequired()
				.HasMaxLength(30);

			builder.Property(x => x.Source)
				.IsRequired()
				.HasMaxLength(2000);

			builder.Property(x => x.Verdict)
				.IsRequired()
				.HasMaxLength(20);

			builder.Property(x => x.PayloadJson)
				.IsRequired()
				.HasColumnType("jsonb")
				.HasDefaultValueSql("'{}'::jsonb");

			builder.HasIndex(x => x.ScoringJobId);
			builder.HasIndex(x => x.Created);
			builder.HasQueryFilter(x => !x.IsDeleted);
		}
	}
}
