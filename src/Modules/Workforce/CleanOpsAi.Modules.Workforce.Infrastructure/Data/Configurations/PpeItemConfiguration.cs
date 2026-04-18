using CleanOpsAi.Modules.Workforce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.Workforce.Infrastructure.Data.Configurations
{
    public class PpeItemConfiguration : IEntityTypeConfiguration<PpeItem>
    {
        public void Configure(EntityTypeBuilder<PpeItem> builder)
        {
            builder.HasKey(c => c.Id);

        }
    }
}
