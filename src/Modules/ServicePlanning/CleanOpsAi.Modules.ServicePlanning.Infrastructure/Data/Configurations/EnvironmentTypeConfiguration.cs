using CleanOpsAi.Modules.ServicePlanning.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanOpsAi.Modules.ServicePlanning.Infrastructure.Data.Configurations
{
	public class EnvironmentTypeConfiguration : IEntityTypeConfiguration<EnvironmentType>
	{
		public void Configure(EntityTypeBuilder<EnvironmentType> builder)
		{
			builder.HasKey(x => x.Id);

			builder.Property(x => x.Name)
				.IsRequired()
				.HasMaxLength(255);

			builder.Property(x => x.Description)
				.HasMaxLength(1000);

			builder.HasQueryFilter(x => !x.IsDeleted);

		}
	}
}
