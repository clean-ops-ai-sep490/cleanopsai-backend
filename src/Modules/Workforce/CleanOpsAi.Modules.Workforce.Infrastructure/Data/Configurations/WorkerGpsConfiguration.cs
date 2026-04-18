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
				.IsRequired(false);

			builder.Property(wg => wg.Longitude)
				.IsRequired(false);

            builder.Property(wg => wg.IsConfirmed)
				.IsRequired(false);

            builder.HasOne(wg => wg.Worker)
				.WithMany(w => w.WorkerGps)
				.HasForeignKey(wg => wg.WorkerId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasIndex(wg => wg.WorkerId);
		}
	}
}
