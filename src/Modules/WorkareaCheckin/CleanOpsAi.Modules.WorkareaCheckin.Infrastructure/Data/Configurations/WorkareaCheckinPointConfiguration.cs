using CleanOpsAi.Modules.WorkareaCheckin.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanOpsAi.Modules.WorkareaCheckin.Infrastructure.Data.Configurations
{
	public class WorkareaCheckinPointConfiguration : IEntityTypeConfiguration<WorkareaCheckinPoint>
	{
		public void Configure(EntityTypeBuilder<WorkareaCheckinPoint> builder)
		{  
			builder.HasKey(x => x.Id);

			builder.Property(x => x.Name)
				.IsRequired()
				.HasMaxLength(100);

			builder.Property(x => x.Code)
				.IsRequired()
				.HasMaxLength(50);

			builder.HasIndex(x => x.Code)
				.IsUnique();

			builder.Property(x => x.IsActive)
				.HasDefaultValue(true);

			builder.HasMany(x => x.AccessDevices)
				.WithOne(x => x.WorkareaCheckinPoint)
				.HasForeignKey(x => x.WorkareaCheckinPointId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasMany(x => x.CheckinRecords)
				.WithOne(x => x.WorkareaCheckinPoint)
				.HasForeignKey(x => x.WorkareaCheckinPointId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasQueryFilter(x => !x.IsDeleted);
		}
	}
}
