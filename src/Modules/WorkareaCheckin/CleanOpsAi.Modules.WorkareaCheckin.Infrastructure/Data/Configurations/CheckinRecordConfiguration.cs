using CleanOpsAi.Modules.WorkareaCheckin.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanOpsAi.Modules.WorkareaCheckin.Infrastructure.Data.Configurations
{
	public class CheckinRecordConfiguration : IEntityTypeConfiguration<CheckinRecord>
	{
		public void Configure(EntityTypeBuilder<CheckinRecord> builder)
		{  
			builder.HasKey(x => x.Id);

			builder.Property(x => x.CheckinType)
				.HasConversion<string>()
				.HasMaxLength(20)
				.IsRequired();

			builder.Property(x => x.Status)
				.HasConversion<string>()
				.HasMaxLength(20)
				.IsRequired();

			builder.Property(x => x.Notes)
				.HasMaxLength(500);
			 
			builder.HasIndex(x => x.WorkerId);
			builder.HasIndex(x => x.CheckinAt);
			builder.HasIndex(x => new { x.WorkareaCheckinPointId, x.CheckinAt });

			builder.HasOne(x => x.WorkareaCheckinPoint)
				.WithMany(x => x.CheckinRecords)
				.HasForeignKey(x => x.WorkareaCheckinPointId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasOne(x => x.AccessDevice)
				.WithMany()
				.HasForeignKey(x => x.AccessDeviceId)
				.OnDelete(DeleteBehavior.SetNull);

			builder.HasQueryFilter(x => !x.IsDeleted);
		}
	}
}
