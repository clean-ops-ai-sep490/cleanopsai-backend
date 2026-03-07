using CleanOpsAi.Modules.ClientManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders; 

namespace CleanOpsAi.Modules.ClientManagement.Infrastructure.Data.Configurations
{
	public class WorkAreaConfiguration : IEntityTypeConfiguration<WorkArea>
	{
		public void Configure(EntityTypeBuilder<WorkArea> builder)
		{
			builder.HasKey(x => x.Id);

			builder.Property(x => x.Name)
				.IsRequired()
				.HasMaxLength(200);

			builder.HasOne(x => x.Zone)
				.WithMany(x => x.WorkAreas)
				.HasForeignKey(x => x.ZoneId);
		}
	}
}
