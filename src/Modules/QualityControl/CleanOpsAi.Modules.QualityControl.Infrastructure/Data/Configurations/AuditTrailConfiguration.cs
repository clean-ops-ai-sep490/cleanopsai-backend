using CleanOpsAi.Modules.QualityControl.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanOpsAi.Modules.QualityControl.Infrastructure.Data.Configurations
{
	public class AuditEventConfiguration : IEntityTypeConfiguration<AuditTrail>
	{
		public void Configure(EntityTypeBuilder<AuditTrail> builder)
		{
			// Table name
			builder.ToTable("audit_trails");

			// Primary key
			builder.HasKey(x => x.Id); 

			builder.Property(x => x.TraceId) 
				.HasMaxLength(100)
				.IsRequired();

			builder.Property(x => x.EntityName) 
				.HasMaxLength(150)
				.IsRequired();

			builder.Property(x => x.EntityId) 
				.HasMaxLength(100)
				.IsRequired();

			builder.Property(x => x.Action) 
				.HasMaxLength(50)
				.IsRequired();

			builder.Property(x => x.UserId) 
				.HasMaxLength(100)
				.IsRequired();

			builder.Property(x => x.ChangedAt) 
				.IsRequired();

			builder.Property(x => x.OldValues) 
				.HasColumnType("jsonb");  

			builder.Property(x => x.NewValues) 
				.HasColumnType("jsonb"); 

			builder.Property(x => x.IpAddress)
				.HasColumnName("ip_address")
				.HasMaxLength(50);

			builder.Property(x => x.Notes)
				.HasColumnName("notes")
				.HasMaxLength(500);

			builder.Property(x => x.Source)
				.HasColumnName("source")
				.HasMaxLength(50)
				.HasDefaultValue("API");
			 
			builder.HasIndex(x => x.TraceId);
			builder.HasIndex(x => new { x.EntityName, x.EntityId });
			builder.HasIndex(x => x.ChangedAt);
			builder.HasIndex(x => x.UserId);
		}
	}
}
