using CleanOpsAi.Modules.QualityControl.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanOpsAi.Modules.QualityControl.Infrastructure.Data.Configurations
{
	public class FcmTokenConfiguration : IEntityTypeConfiguration<FcmToken>
	{
		public void Configure(EntityTypeBuilder<FcmToken> builder)
		{
			builder.HasKey(x => x.Id);

			builder.Property(x => x.Token)
			   .IsRequired()
			   .HasMaxLength(500);

			builder.Property(x => x.Platform)
			   .IsRequired();

			builder.Property(x => x.UniqueId)
			   .HasMaxLength(100);

			builder.HasQueryFilter(x => !x.IsDeleted);
		}
	}
}
