using CleanOpsAi.Modules.WorkareaCheckin.Domain.Entities; 
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders; 

namespace CleanOpsAi.Modules.WorkareaCheckin.Infrastructure.Data.Configurations
{
	public class AccessDeviceConfiguration : IEntityTypeConfiguration<AccessDevice>
	{
		public void Configure(EntityTypeBuilder<AccessDevice> builder)
		{  
			builder.HasKey(x => x.Id);

			builder.Property(x => x.Name)
				.HasMaxLength(100);

			builder.Property(x => x.Code)
				.HasMaxLength(50);

			builder.Property(x => x.Identifier)
				.HasMaxLength(200);

			builder.Property(x => x.DeviceType)
				.HasConversion<string>()
				.HasMaxLength(20)
				.IsRequired();

			builder.Property(x => x.Status)
				.HasConversion<string>()
				.HasMaxLength(20)
				.IsRequired();  

			builder.Property(x => x.InstallationLocation)
				.HasMaxLength(255);

			builder.OwnsOne(x => x.BleInfo, ble =>
			{
				ble.Property(b => b.ServiceUuid);

				ble.Property(b => b.TxPower);
				ble.Property(b => b.RssiThreshold);
				ble.Property(b => b.LastRssi); 

				ble.Property(b => b.BatteryLevel);
				ble.Property(b => b.LastSeenAt);
				ble.Property(b => b.LastMaintenanceAt);
			});

			builder.HasQueryFilter(x => !x.IsDeleted);
		}
	}
}
