using CleanOpsAi.Modules.ServicePlanning.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanOpsAi.Modules.ServicePlanning.Infrastructure.Data.Configurations
{
	public class SopConfiguration : IEntityTypeConfiguration<Sop>
	{
		public void Configure(EntityTypeBuilder<Sop> builder)
		{
			builder.HasKey(x => x.Id);

			builder.Property(x => x.Name)
				.IsRequired()
				.HasMaxLength(255);

			builder.Property(x => x.Description)
				.HasMaxLength(1000); 

			builder.Property(x => x.EnvironmentType)
				.IsRequired();

			builder.Property(x => x.IsRequiredSkill)
				.IsRequired();

			builder.Property(x => x.IsRequiredCertification)
				.IsRequired();

			builder.Property(x => x.Version)
				.IsRequired();

			builder.HasMany(x => x.SopSteps)
				.WithOne(x => x.Sop)
				.HasForeignKey(x => x.SopId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
