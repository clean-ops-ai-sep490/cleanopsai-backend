using CleanOpsAi.Modules.ClientManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders; 

namespace CleanOpsAi.Modules.ClientManagement.Infrastructure.Data.Configurations
{
	public class ContractConfiguration : IEntityTypeConfiguration<Contract>
	{
		public void Configure(EntityTypeBuilder<Contract> builder)
		{
			builder.HasKey(x => x.Id);

			builder.Property(x => x.Name)
				.IsRequired()
				.HasMaxLength(200);
			 

			builder.HasOne(x => x.Client)
				.WithMany(x => x.Contracts)
				.HasForeignKey(x => x.ClientId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasMany(x => x.ContractShifts)
				.WithOne(x => x.Contract)
				.HasForeignKey(x => x.ContractId);

			builder.HasMany(x => x.Slas)
				.WithOne(x => x.Contract)
				.HasForeignKey(x => x.ContractId);
		}
	}
}
