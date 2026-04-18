using CleanOpsAi.Modules.QualityControl.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanOpsAi.Modules.QualityControl.Infrastructure.Data.Configurations
{
	public class AppNotificationConfiguration : IEntityTypeConfiguration<AppNotification>
	{
		public void Configure(EntityTypeBuilder<AppNotification> builder)
		{
			builder.HasKey(x => x.Id);

			builder.Property(x => x.Title)
			   .IsRequired()
			   .HasMaxLength(500);

			builder.Property(x => x.Body)
			   .IsRequired(false)         
			   .HasMaxLength(2000);

			builder.Property(x=> x.Body).IsRequired();

			builder.Property(x => x.Payload)
			   .HasColumnType("jsonb")
			   .HasDefaultValueSql("'{}'::jsonb") ;

			builder.HasIndex(x => x.Priority);

			builder.HasQueryFilter(x => !x.IsDeleted);

		}
	}
}
