using CleanOpsAi.Modules.TaskOperations.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders; 

namespace CleanOpsAi.Modules.TaskOperations.Infrastructure.Data.Configurations
{
	public class IssueReportConfiguration : IEntityTypeConfiguration<IssueReport>
	{
		public void Configure(EntityTypeBuilder<IssueReport> builder)
		{
			builder.HasKey(x => x.Id);

			builder.Property(x => x.Description)
				.IsRequired()
				.HasMaxLength(2000);

			builder.Property(x => x.Status)
				.IsRequired();

			builder.Property(x => x.ResolvedByUserId);

			builder.HasOne(x => x.TaskAssignment)
				.WithMany(x => x.IssueReports)
				.HasForeignKey(x => x.TaskAssignmentId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasIndex(x => x.TaskAssignmentId);
			builder.HasIndex(x => x.ReportedByWorkerId);

			builder.HasQueryFilter(x => !x.IsDeleted);
		}
	}
}
