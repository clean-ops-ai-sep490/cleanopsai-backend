using CleanOpsAi.Modules.ClientManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders; 

namespace CleanOpsAi.Modules.ClientManagement.Infrastructure.Data.Configurations
{
	public class ZoneConfiguration : IEntityTypeConfiguration<Zone>
	{
		public void Configure(EntityTypeBuilder<Zone> builder)
		{
			builder.HasKey(x => x.Id);

			builder.Property(x => x.Name)
				.IsRequired()
				.HasMaxLength(200);

			builder.HasOne(x => x.Location)
				.WithMany(x => x.Zones)
				.HasForeignKey(x => x.LocationId);

			builder.HasMany(x => x.WorkAreas)
				.WithOne(x => x.Zone)
				.HasForeignKey(x => x.ZoneId);
		}
	}
}
