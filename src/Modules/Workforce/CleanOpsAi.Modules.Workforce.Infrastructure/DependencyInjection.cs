using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Infrastructure;
using CleanOpsAi.Modules.Workforce.Application.Configurations;
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

		builder.Services.AddHttpClient<IAddressKitService, AddressKitService>(client =>
		{
			client.BaseAddress = new Uri("https://production.cas.so/address-kit/2025-07-01/");
			client.Timeout = TimeSpan.FromSeconds(30);
		});


		// Repositories
		builder.Services.AddScoped<IWorkerRepository, WorkerRepository>();
		builder.Services.AddScoped<ICertificationRepository, CertificationRepository>();
		builder.Services.AddScoped<IEquipmentRepository, EquipmentRepository>();
		builder.Services.AddScoped<ISkillRepository, SkillRepository>();
		builder.Services.AddScoped<IWorkerCertificationRepository, WorkerCertificationRepository>();
		builder.Services.AddScoped<IWorkerSkillRepository, WorkerSkillRepository>();
		builder.Services.AddScoped<IWorkerGpsRepository, WorkerGpsRepository>();
		builder.Services.AddScoped<IWorkAreaSupervisorRepository, WorkAreaSupervisorRepository>(); 
		builder.Services.AddScoped<IPpeItemRepository, PpeItemRepository>();

        // Services
        builder.Services.AddScoped<IWorkerService, WorkerService>();
		builder.Services.AddScoped<ICertificationService, CertificationService>();
		builder.Services.AddScoped<IEquipmentService, EquipmentService>();
		builder.Services.AddScoped<ISkillService, SkillService>();
		builder.Services.AddScoped<IWorkerCertificationService, WorkerCertificationService>();
		builder.Services.AddScoped<IWorkerSkillService, WorkerSkillService>();
		builder.Services.AddScoped<IWorkerGpsService, WorkerGpsService>();
		builder.Services.AddScoped<IWorkAreaSupervisorService, WorkAreaSupervisorService>();
		builder.Services.AddScoped<IPpeItemService, PpeItemService>();
        builder.Services.AddScoped<IGeminiService, GeminiService>();

        // Dependency Injection for Azure Blob Storage Service
        builder.Services.AddScoped<IFileStorageService, AzureBlobStorageService>();
		builder.Services.AddHttpClient<GoongMapService>();
		builder.Services.AddScoped<IGoongMapService, GoongMapService>();


	}


}