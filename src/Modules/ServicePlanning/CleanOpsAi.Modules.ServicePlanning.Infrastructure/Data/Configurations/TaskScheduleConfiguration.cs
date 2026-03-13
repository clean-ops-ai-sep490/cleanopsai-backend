using CleanOpsAi.Modules.ServicePlanning.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders; 

namespace CleanOpsAi.Modules.ServicePlanning.Infrastructure.Data.Configurations
{
	public class TaskScheduleConfiguration : IEntityTypeConfiguration<TaskSchedule>
	{
		public void Configure(EntityTypeBuilder<TaskSchedule> builder)
		{
			builder.HasKey(x => x.Id);

			builder.Property(x => x.Name)
				.IsRequired()
				.HasMaxLength(255);

			builder.Property(x => x.Description) 
				.HasMaxLength(1000);

			builder.Property(x => x.Metadata)
				.HasColumnType("jsonb");

			builder.Property(x => x.RecurrenceConfig)
				.IsRequired()
				.HasColumnType("jsonb");

			builder.Property(x => x.RecurrenceType)
				.IsRequired();

			builder.HasIndex(x => x.SopId);

			builder.HasOne(x => x.Sop)
				.WithMany(x => x.TaskSchedules)
				.HasForeignKey(x => x.SopId)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}
