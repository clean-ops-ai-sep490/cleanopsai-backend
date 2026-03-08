using CleanOpsAi.Modules.ClientManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders; 

namespace CleanOpsAi.Modules.ClientManagement.Infrastructure.Data.Configurations
{
	public class SlaTaskConfiguration : IEntityTypeConfiguration<SlaTask>
	{
		public void Configure(EntityTypeBuilder<SlaTask> builder)
		{
			builder.HasKey(x => x.Id);

			builder.Property(x => x.Name)
				.IsRequired()
				.HasMaxLength(200);

			builder.Property(x => x.RecurrenceType)
				.IsRequired();

			builder.Property(x => x.RecurrenceConfig)
				.IsRequired()
				.HasColumnType("jsonb"); 

			builder.HasOne(x => x.Sla)
				.WithMany()
				.HasForeignKey(x => x.SlaId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
