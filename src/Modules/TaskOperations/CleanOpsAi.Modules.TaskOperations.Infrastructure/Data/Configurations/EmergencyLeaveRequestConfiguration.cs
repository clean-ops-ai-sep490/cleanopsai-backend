using CleanOpsAi.Modules.TaskOperations.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders; 

namespace CleanOpsAi.Modules.TaskOperations.Infrastructure.Data.Configurations
{
	public class EmergencyLeaveRequestConfiguration : IEntityTypeConfiguration<EmergencyLeaveRequest>
	{
		public void Configure(EntityTypeBuilder<EmergencyLeaveRequest> builder)
		{
			builder.HasKey(x => x.Id);

			builder.Property(x => x.AudioUrl)
				.HasMaxLength(1000);

			builder.Property(x => x.Transcription)
				.HasColumnType("text");

			builder.Property(x => x.Status)
				.IsRequired();

			builder.Property(x => x.ReviewedById)
				.HasMaxLength(450);

			builder.HasOne(x => x.TaskAssignment)
				.WithMany(x => x.EmergencyLeaveRequests)
				.HasForeignKey(x => x.TaskAssignmentId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
