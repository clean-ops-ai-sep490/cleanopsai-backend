using CleanOpsAi.Modules.Scoring.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanOpsAi.Modules.Scoring.Infrastructure.Data.Configurations
{
	public class ScoringAnnotationConfiguration : IEntityTypeConfiguration<ScoringAnnotation>
	{
		public void Configure(EntityTypeBuilder<ScoringAnnotation> builder)
		{
			builder.HasKey(x => x.Id);

			builder.Property(x => x.LabelsJson)
				.IsRequired()
				.HasColumnType("jsonb")
				.HasDefaultValueSql("'[]'::jsonb");

			builder.Property(x => x.ReviewerNote)
				.HasMaxLength(2000);

			builder.HasIndex(x => x.CandidateId)
				.IsUnique();
			builder.HasIndex(x => x.Created);
			builder.HasQueryFilter(x => !x.IsDeleted);
		}
	}
}
