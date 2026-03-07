using CleanOpsAi.Modules.ClientManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders; 

namespace CleanOpsAi.Modules.ClientManagement.Infrastructure.Data.Configurations
{
	public class ContractShiftConfiguration : IEntityTypeConfiguration<ContractShift>
	{
		public void Configure(EntityTypeBuilder<ContractShift> builder)
		{
			builder.HasKey(x => x.Id); 

			builder.HasOne(x => x.Contract)
				.WithMany(x => x.ContractShifts)
				.HasForeignKey(x => x.ContractId);

			builder.HasOne(x => x.WorkArea)
				.WithMany()
				.HasForeignKey(x => x.WorkAreaId)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}
