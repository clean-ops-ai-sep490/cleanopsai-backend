using CleanOpsAi.Modules.ClientManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders; 

namespace CleanOpsAi.Modules.ClientManagement.Infrastructure.Data.Configurations
{
	public class SlaShiftConfiguration : IEntityTypeConfiguration<SlaShift>
	{
		public void Configure(EntityTypeBuilder<SlaShift> builder)
		{
			builder.HasKey(x => x.Id);

			builder.Property(x => x.Name)
				.IsRequired()
				.HasMaxLength(100);   

			builder.HasOne(x => x.Sla)
				.WithMany()
				.HasForeignKey(x => x.SlaId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
