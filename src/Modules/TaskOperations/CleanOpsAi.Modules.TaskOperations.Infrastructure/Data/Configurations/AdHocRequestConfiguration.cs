using CleanOpsAi.Modules.TaskOperations.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders; 

namespace CleanOpsAi.Modules.TaskOperations.Infrastructure.Data.Configurations
{
	public class AdHocRequestConfiguration : IEntityTypeConfiguration<AdHocRequest>
	{
		public void Configure(EntityTypeBuilder<AdHocRequest> builder)
		{
			builder.HasKey(x => x.Id);

			builder.Property(x => x.RequestedByUserId)
				.IsRequired()
				.HasMaxLength(450);

			builder.Property(x => x.Reason)
				.HasMaxLength(1000); 

			builder.Property(x => x.Status)
				.IsRequired();

			builder.Property(x => x.ReviewedByUserId)
				.HasMaxLength(450);

			builder.HasOne(x => x.TaskAssignment)
				.WithMany(x => x.AdHocRequests)
				.HasForeignKey(x => x.TaskAssignmentId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
