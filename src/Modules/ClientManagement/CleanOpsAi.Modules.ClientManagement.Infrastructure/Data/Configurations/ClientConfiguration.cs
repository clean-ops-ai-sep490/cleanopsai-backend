using CleanOpsAi.Modules.ClientManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanOpsAi.Modules.ClientManagement.Infrastructure.Data.Configurations
{
	public class ClientConfiguration : IEntityTypeConfiguration<Client>
	{
		public void Configure(EntityTypeBuilder<Client> builder)
		{
			builder.HasKey(x => x.Id);

			builder.Property(x => x.Name)
			       .IsRequired()
				   .HasMaxLength(200);

			builder.Property(x => x.Email)
				   .IsRequired()
				   .HasMaxLength(200);

			builder.HasMany(x => x.Contracts)
					.WithOne(x => x.Client)
			.HasForeignKey(x => x.ClientId)
					.OnDelete(DeleteBehavior.Cascade);

			builder.HasMany(x => x.Locations)
				   .WithOne(x => x.Client)
				   .HasForeignKey(x => x.ClientId)
				   .OnDelete(DeleteBehavior.Cascade);
		}
	}
	
}
