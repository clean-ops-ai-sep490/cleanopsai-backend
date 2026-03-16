using CleanOpsAi.Modules.Workforce.Application.Configurations;
using CleanOpsAi.Modules.Workforce.Application.Consumers;
using CleanOpsAi.Modules.Workforce.Application.Interfaces;
using CleanOpsAi.Modules.Workforce.Application.Services;
using CleanOpsAi.Modules.Workforce.Infrastructure.Data;
using CleanOpsAi.Modules.Workforce.Infrastructure.Repositories;
using CleanOpsAi.Modules.Workforce.Infrastructure.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection;
public static class DependencyInjection
{
	public static void InfrastructureWorkforceModule(this IHostApplicationBuilder builder)
	{
		builder.Services.AddSingleton(TimeProvider.System);

		builder.Services.AddDbContext<WorkforceDbContext>(options =>
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

		builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(AssemblyReference.Assembly));

        // Repositories
        builder.Services.AddScoped<IWorkerRepository, WorkerRepository>();
		builder.Services.AddScoped<ICertificationRepository, CertificationRepository>();
		builder.Services.AddScoped<IEquipmentRepository, EquipmentRepository>();
		builder.Services.AddScoped<ISkillRepository, SkillRepository>();

        // Services
        builder.Services.AddScoped<IWorkerService, WorkerService>();
		builder.Services.AddScoped<ICertificationService, CertificationService>();
		builder.Services.AddScoped<IEquipmentService, EquipmentService>();
		builder.Services.AddScoped<ISkillService, SkillService>();

        // Dependency Injection for Azure Blob Storage Service
        builder.Services.AddScoped<IFileStorageService, AzureBlobStorageService>();
    }


}