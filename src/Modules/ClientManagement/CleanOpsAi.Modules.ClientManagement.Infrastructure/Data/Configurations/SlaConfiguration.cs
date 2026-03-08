using CleanOpsAi.Modules.ClientManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders; 

namespace CleanOpsAi.Modules.ClientManagement.Infrastructure.Data.Configurations
{
	public class SlaConfiguration : IEntityTypeConfiguration<Sla>
	{
		public void Configure(EntityTypeBuilder<Sla> builder)
		{
			builder.HasKey(x => x.Id);

			builder.Property(x => x.Name)
				.IsRequired()
				.HasMaxLength(200);

			builder.Property(x => x.Description)
			.HasMaxLength(1000);

			builder.HasOne(x => x.Contract)
				.WithMany(x => x.Slas)
				.HasForeignKey(x => x.ContractId);

			builder.HasOne(x => x.WorkArea)
				.WithMany(x => x.Slas)
				.HasForeignKey(x => x.WorkAreaId)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}
