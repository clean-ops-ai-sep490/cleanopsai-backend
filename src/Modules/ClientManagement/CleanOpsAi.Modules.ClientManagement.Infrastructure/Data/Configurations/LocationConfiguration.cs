using CleanOpsAi.Modules.ClientManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders; 

namespace CleanOpsAi.Modules.ClientManagement.Infrastructure.Data.Configurations
{
	public class LocationConfiguration : IEntityTypeConfiguration<Location>
	{
		public void Configure(EntityTypeBuilder<Location> builder)
		{
			builder.HasKey(x => x.Id);

			builder.Property(x => x.Name)
				.IsRequired()
				.HasMaxLength(200);

			builder.Property(x => x.Address)
				.IsRequired();

			builder.HasOne(x => x.Client)
				.WithMany(x => x.Locations)
				.HasForeignKey(x => x.ClientId);

			builder.HasMany(x => x.Zones)
				.WithOne(x => x.Location)
				.HasForeignKey(x => x.LocationId);
		}
	}
}
