using CleanOpsAi.Modules.WorkareaCheckin.Application.Common.Mappings;
using CleanOpsAi.Modules.WorkareaCheckin.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection;
public static class DependencyInjection
{
	public static void InfrastructureWorkAreaCheckinModule(this IHostApplicationBuilder builder)
	{
		builder.Services.AddSingleton(TimeProvider.System);

		builder.Services.AddDbContext<WorkareaCheckinDbContext>(options =>
		{
			options.UseNpgsql(
				 builder.Configuration["ConnectionStrings:DefaultConnection"]
			)
				   .UseSnakeCaseNamingConvention()
				   .EnableSensitiveDataLogging()
				   .EnableDetailedErrors();
			options.EnableSensitiveDataLogging();
			options.LogTo(Console.WriteLine, new[] { DbLoggerCategory.Database.Command.Name }, LogLevel.Information);
		});

		builder.Services.AddAutoMapper(cfg => cfg.LicenseKey = builder.Configuration["AutoMapper:Key"], typeof(MappingProfile));
	}
}