using CleanOpsAi.Modules.Scoring.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanOpsAi.Modules.Scoring.Infrastructure.Data.Configurations
{
	public class ScoringAnnotationCandidateConfiguration : IEntityTypeConfiguration<ScoringAnnotationCandidate>
	{
		public void Configure(EntityTypeBuilder<ScoringAnnotationCandidate> builder)
		{
			builder.HasKey(x => x.Id);

			builder.Property(x => x.RequestId)
				.IsRequired()
				.HasMaxLength(100);

			builder.Property(x => x.EnvironmentKey)
				.IsRequired()
				.HasMaxLength(50);

			builder.Property(x => x.ImageUrl)
				.IsRequired()
				.HasMaxLength(2000);

			builder.Property(x => x.VisualizationBlobUrl)
				.HasMaxLength(2000);

			builder.Property(x => x.OriginalVerdict)
				.IsRequired()
				.HasMaxLength(20);

			builder.Property(x => x.ReviewedVerdict)
				.IsRequired()
				.HasMaxLength(20);

			builder.Property(x => x.SourceType)
				.IsRequired()
				.HasMaxLength(100);

			builder.Property(x => x.SnapshotBlobKey)
				.HasMaxLength(2000);

			builder.Property(x => x.MetadataBlobKey)
				.HasMaxLength(2000);

			builder.HasIndex(x => x.ResultId)
				.IsUnique();
			builder.HasIndex(x => x.CandidateStatus);
			builder.HasIndex(x => x.CreatedAtUtc);
			builder.HasIndex(x => x.EnvironmentKey);
			builder.HasIndex(x => x.AssignedToUserId);
			builder.HasQueryFilter(x => !x.IsDeleted);

			builder.HasOne(x => x.Result)
				.WithMany(x => x.AnnotationCandidates)
				.HasForeignKey(x => x.ResultId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasOne(x => x.Annotation)
				.WithOne(x => x.Candidate)
				.HasForeignKey<ScoringAnnotation>(x => x.CandidateId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
