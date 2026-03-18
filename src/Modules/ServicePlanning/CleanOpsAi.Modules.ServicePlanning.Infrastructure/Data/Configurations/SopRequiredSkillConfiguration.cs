using CleanOpsAi.Modules.ServicePlanning.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanOpsAi.Modules.ServicePlanning.Infrastructure.Data.Configurations
{
	public class SopRequiredSkillConfiguration : IEntityTypeConfiguration<SopRequiredSkill>
	{
		public void Configure(EntityTypeBuilder<SopRequiredSkill> builder)
		{
			builder.HasKey(x => new { x.SopId, x.SkillId });

			builder.HasOne(x => x.Sop)
				.WithMany(x => x.SopRequiredSkills)
				.HasForeignKey(x => x.SopId);
		}
	}
}
