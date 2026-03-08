using CleanOpsAi.Modules.Workforce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanOpsAi.Modules.Workforce.Infrastructure.Data.Configurations
{
	public class WorkerGpsConfiguration : IEntityTypeConfiguration<WorkerGps>
	{
		public void Configure(EntityTypeBuilder<WorkerGps> builder)
		{

			builder.HasKey(wg => wg.Id);

			builder.Property(wg => wg.WorkerId)
				.IsRequired();

			builder.Property(wg => wg.Latitude)
				.IsRequired();

			builder.Property(wg => wg.Longitude)
				.IsRequired();

			builder.HasOne(wg => wg.Worker)
				.WithMany(w => w.WorkerGps)
				.HasForeignKey(wg => wg.WorkerId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasIndex(wg => wg.WorkerId);
		}
	}
}
