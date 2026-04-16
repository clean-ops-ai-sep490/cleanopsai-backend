using CleanOpsAi.Modules.TaskOperations.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.TaskOperations.Infrastructure.Data.Configurations
{
    public class EquipmentRequestItemConfiguration : IEntityTypeConfiguration<EquipmentRequestItem>
    {
        public void Configure(EntityTypeBuilder<EquipmentRequestItem> builder)
        {

            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.EquipmentRequest)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.EquipmentRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(x => x.EquipmentId)
                .IsRequired();

            builder.Property(x => x.Quantity)
                .IsRequired();

            builder.HasIndex(x => x.EquipmentRequestId);
            builder.HasIndex(x => x.EquipmentId);

            builder.HasIndex(x => new { x.EquipmentRequestId, x.EquipmentId })
                .IsUnique();
        }
    }
}
