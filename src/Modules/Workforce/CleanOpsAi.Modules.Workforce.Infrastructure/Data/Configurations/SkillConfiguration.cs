using CleanOpsAi.Modules.Workforce.Domain.Enitites;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanOpsAi.Modules.Workforce.Infrastructure.Data.Configurations
{
	public class SkillConfiguration : IEntityTypeConfiguration<Skill>
	{
		public void Configure(EntityTypeBuilder<Skill> builder)
		{
			builder.HasKey(s => s.Id);

			builder.Property(s => s.Name).IsRequired().HasMaxLength(200);

		
		}
	}
}
