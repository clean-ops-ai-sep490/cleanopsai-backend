using CleanOpsAi.Modules.WorkareaCheckin.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.WorkareaCheckin.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.WorkareaCheckin.Application.Common.Mappings;
using CleanOpsAi.Modules.WorkareaCheckin.Application.Services;
using CleanOpsAi.Modules.WorkareaCheckin.Infrastructure.Data;
using CleanOpsAi.Modules.WorkareaCheckin.Infrastructure.Repositories;
using CleanOpsAi.Modules.WorkareaCheckin.Infrastructure.Services;
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

		builder.Services.AddScoped<IWorkareaCheckinPointRepository, WorkareaCheckinPointRepository>();
		builder.Services.AddScoped<IAccessDeviceRepository, AccessDeviceRepository>();
		builder.Services.AddScoped<ICheckinRecordRepository, CheckinRecordRepository>();


		builder.Services.AddScoped<IWorkareaCheckinPointService, WorkareaCheckinPointService>();
		builder.Services.AddScoped<ICheckinRecordService, CheckinRecordService>();
		builder.Services.AddScoped<IAccessDeviceService, AccessDeviceService>();

		builder.Services.AddScoped<IQrCodeService, QrCodeService>();

	}
}