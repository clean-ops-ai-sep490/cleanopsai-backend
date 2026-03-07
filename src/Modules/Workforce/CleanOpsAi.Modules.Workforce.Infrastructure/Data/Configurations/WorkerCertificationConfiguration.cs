using CleanOpsAi.Modules.Workforce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders; 

namespace CleanOpsAi.Modules.Workforce.Infrastructure.Data.Configurations
{
	public class WorkerCertificationConfiguration : IEntityTypeConfiguration<WorkerCertification>
	{
		public void Configure(EntityTypeBuilder<WorkerCertification> builder)
		{ 
			builder.HasKey(wc => new { wc.WorkerId, wc.CertificationId });
			 
			builder.HasOne(wc => wc.Worker)
				.WithMany(w => w.WorkerCertifications)
				.HasForeignKey(wc => wc.WorkerId)
				.OnDelete(DeleteBehavior.Cascade);
			 
			builder.HasOne(wc => wc.Certification)
				.WithMany(c => c.WorkerCertifications)
				.HasForeignKey(wc => wc.CertificationId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.Property(wc => wc.IssuedDate).IsRequired();
			builder.Property(wc => wc.ExpiredAt).IsRequired(false);
		}
	}
}
