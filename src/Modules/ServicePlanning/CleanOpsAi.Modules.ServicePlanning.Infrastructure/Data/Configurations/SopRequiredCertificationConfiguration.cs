using CleanOpsAi.Modules.ServicePlanning.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanOpsAi.Modules.ServicePlanning.Infrastructure.Data.Configurations
{
	public class SopRequiredCertificationConfiguration : IEntityTypeConfiguration<SopRequiredCertification>
	{
		public void Configure(EntityTypeBuilder<SopRequiredCertification> builder)
		{
			builder.HasKey(x => new { x.SopId, x.CertificationId });

			builder.HasOne(x => x.Sop)
				.WithMany(x => x.SopRequiredCertifications)
				.HasForeignKey(x => x.SopId);

		}
	}
}
