using CleanOpsAi.Modules.TaskOperations.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders; 

namespace CleanOpsAi.Modules.TaskOperations.Infrastructure.Data.Configurations
{
	public class EquipmentRequestConfiguration : IEntityTypeConfiguration<EquipmentRequest>
	{
		public void Configure(EntityTypeBuilder<EquipmentRequest> builder)
		{
			builder.HasKey(x => x.Id);

			builder.Property(x => x.Reason)
				.HasMaxLength(1000);

			builder.Property(x => x.ReviewedByUserId);

			builder.Property(x => x.Status)
				.IsRequired();

			builder.HasOne<TaskAssignment>()
				.WithMany(x => x.EquipmentRequests)
				.HasForeignKey(x => x.TaskAssignmentId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasIndex(x => x.TaskAssignmentId);
			builder.HasIndex(x => x.WorkerId);

			builder.HasQueryFilter(x => !x.IsDeleted);
		}
	}
}
