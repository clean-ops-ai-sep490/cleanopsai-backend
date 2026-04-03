using CleanOpsAi.Modules.Workforce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders; 

namespace CleanOpsAi.Modules.Workforce.Infrastructure.Data.Configurations
{
    public class WorkAreaSupervisorConfiguration : IEntityTypeConfiguration<WorkAreaSupervisor>
    {
        public void Configure(EntityTypeBuilder<WorkAreaSupervisor> builder)
        {
            // PK
            builder.HasKey(x => x.Id);

            // Properties
            builder.Property(x => x.WorkAreaId)
                   .IsRequired(false);

            builder.Property(x => x.WorkerId)
                   .IsRequired(false);

            builder.Property(x => x.UserId)
                   .IsRequired()
                   .HasMaxLength(450);

            // Relationship (optional vì WorkerId nullable)
            builder.HasOne(x => x.Worker)
                   .WithMany() // hoặc WithMany(w => w.WorkAreaSupervisors)
                   .HasForeignKey(x => x.WorkerId)
                   .OnDelete(DeleteBehavior.SetNull); // vì FK nullable

            // Index
            builder.HasIndex(x => x.WorkAreaId);
            builder.HasIndex(x => x.WorkerId);
            builder.HasIndex(x => x.UserId);

            // Unique constraint (cẩn thận với nullable)
            builder.HasIndex(x => new { x.WorkAreaId, x.WorkerId, x.UserId })
               .IsUnique()
               .HasFilter("\"work_area_id\" IS NOT NULL AND \"worker_id\" IS NOT NULL");
        }
    }
}
