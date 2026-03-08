using CleanOpsAi.Modules.Workforce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanOpsAi.Modules.Workforce.Infrastructure.Data.Configurations
{
	public class EquipmentConfiguration : IEntityTypeConfiguration<Equipment>
	{
		public void Configure(EntityTypeBuilder<Equipment> builder)
		{
			builder.HasKey(e => e.Id);

			builder.Property(e => e.Name)
				.IsRequired()
				.HasMaxLength(100);
		}
	}
}
