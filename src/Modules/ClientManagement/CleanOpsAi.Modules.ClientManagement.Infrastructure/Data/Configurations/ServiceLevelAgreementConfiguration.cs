using CleanOpsAi.Modules.ClientManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders; 

namespace CleanOpsAi.Modules.ClientManagement.Infrastructure.Data.Configurations
{
	public class ServiceLevelAgreementConfiguration : IEntityTypeConfiguration<ServiceLevelAgreement>
	{
		public void Configure(EntityTypeBuilder<ServiceLevelAgreement> builder)
		{
			builder.HasKey(x => x.Id);

			builder.Property(x => x.Name)
				.IsRequired()
				.HasMaxLength(200);

			builder.HasOne(x => x.Contract)
				.WithMany(x => x.ServiceLevelAgreements)
				.HasForeignKey(x => x.ContractId);

			builder.HasOne(x => x.WorkArea)
				.WithMany()
				.HasForeignKey(x => x.WorkAreaId)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}
