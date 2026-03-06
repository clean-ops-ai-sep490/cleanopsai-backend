using CleanOpsAi.Modules.Workforce.Domain.Enitites;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanOpsAi.Modules.Workforce.Infrastructure.Data.Configurations
{
	public class CertificationConfiguration : IEntityTypeConfiguration<Certification>
	{
		public void Configure(EntityTypeBuilder<Certification> builder)
		{
			builder.HasKey(c => c.Id);
			 
		}
	}
}
