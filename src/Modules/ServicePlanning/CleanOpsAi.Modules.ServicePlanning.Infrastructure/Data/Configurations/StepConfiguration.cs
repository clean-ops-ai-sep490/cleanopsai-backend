using CleanOpsAi.Modules.ServicePlanning.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanOpsAi.Modules.ServicePlanning.Infrastructure.Data.Configurations
{
	public class StepConfiguration : IEntityTypeConfiguration<Step>
	{
		public void Configure(EntityTypeBuilder<Step> builder)
		{
			builder.HasKey(x => x.Id);

			builder.Property(x => x.ActionKey)
				.IsRequired()
				.HasMaxLength(100);

			builder.Property(x => x.Name)
				.IsRequired()
				.HasMaxLength(255);

			builder.Property(x => x.Description)
				.HasMaxLength(1000);

			builder.Property(x => x.ConfigSchema)
				.IsRequired()
				.HasColumnType("jsonb");  

			builder.HasIndex(x => x.ActionKey)
				.IsUnique();
		}
	}
}
