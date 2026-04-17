using CleanOpsAi.Modules.QualityControl.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanOpsAi.Modules.QualityControl.Infrastructure.Data.Configurations
{
	public class NotificationRecipientConfiguration : IEntityTypeConfiguration<NotificationRecipient>
	{
		public void Configure(EntityTypeBuilder<NotificationRecipient> builder)
		{
			builder.HasKey(x => x.Id);

			builder.Property(x => x.NotificationId)
			   .IsRequired();

			builder.Property(x => x.IsRead)
			   .HasDefaultValue(false)
			   .IsRequired();

			builder.Property(x => x.RecipientId).IsRequired(false);

			builder.HasIndex(x => new { x.NotificationId, x.RecipientId })
				.IsUnique()
				.HasFilter("is_deleted = false AND recipient_id IS NOT NULL");

			builder.HasOne(x=> x.AppNotification)
				.WithMany(x=> x.NotificationRecipients)
				.HasForeignKey(x=>x.NotificationId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasQueryFilter(x => !x.IsDeleted);
		}
	}
}
