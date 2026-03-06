using CleanOpsAi.Modules.Workforce.Domain.Enitites;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders; 

namespace CleanOpsAi.Modules.Workforce.Infrastructure.Data.Configurations
{
	public class WorkerSkillConfiguration : IEntityTypeConfiguration<WorkerSkill>
	{
		public void Configure(EntityTypeBuilder<WorkerSkill> builder)
		{
			builder.HasKey(ws => new { ws.WorkerId, ws.SkillId });

			builder.HasOne(ws => ws.Worker)
				.WithMany(w => w.WorkerSkills)
				.HasForeignKey(ws => ws.WorkerId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasOne(ws => ws.Skill)
				.WithMany(s => s.WorkerSkills)
				.HasForeignKey(ws => ws.SkillId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.Property(ws => ws.SkillLevel)
				.IsRequired();
		}
	}
}
