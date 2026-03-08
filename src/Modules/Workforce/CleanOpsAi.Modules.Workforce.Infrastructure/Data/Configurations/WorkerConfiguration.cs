using CleanOpsAi.Modules.Workforce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanOpsAi.Modules.Workforce.Infrastructure.Data.Configurations
{
	public class WorkerConfiguration : IEntityTypeConfiguration<Worker>
	{
		public void Configure(EntityTypeBuilder<Worker> builder)
		{ 
			builder.HasKey(w => w.Id);

			builder.Property(w => w.UserId).IsRequired(); 
			 
			builder.Property(w => w.Latitude);
			builder.Property(w => w.Longitude);

			builder.HasMany(w => w.WorkerGps)
				   .WithOne(wg => wg.Worker)
				   .HasForeignKey(wg => wg.WorkerId);

		}
	}
}
