using CleanOpsAi.Modules.ClientManagement.Application.Configurations;
using CleanOpsAi.Modules.ClientManagement.Application.Interfaces;
using CleanOpsAi.Modules.ClientManagement.Application.Services;
using CleanOpsAi.Modules.ClientManagement.Infrastructure.Data;
using CleanOpsAi.Modules.ClientManagement.Infrastructure.Repositories;
using CleanOpsAi.Modules.ClientManagement.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;

namespace Microsoft.Extensions.DependencyInjection;
public static class DependencyInjection
{
	public static void InfrastructureClientManagementModule(this IHostApplicationBuilder builder)
	{
		builder.Services.AddSingleton(TimeProvider.System);

		builder.Services.AddDbContext<ClientManagementDbContext>(options =>
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

        // Dependency Injection for Repositories
		builder.Services.AddScoped<IClientRepository, ClientRepository>();
        builder.Services.AddScoped<IContractRepository, ContractRepository>();
		builder.Services.AddScoped<ILocationRepository, LocationRepository>();
		builder.Services.AddScoped<IZoneRepository, ZoneRepository>();
		builder.Services.AddScoped<IWorkAreaRepository, WorkAreaRepository>();
		builder.Services.AddScoped<IWorkAreaDetailRepository, WorkAreaDetailRepository>();
        builder.Services.AddScoped<ISlaRepository, SlaRepository>();

        // Dependency Injection for Services
        builder.Services.AddScoped<IClientService, ClientService>();
        builder.Services.AddScoped<IContractService, ContractService>();
		builder.Services.AddScoped<ILocationService, LocationService>();
		builder.Services.AddScoped<IZoneService, ZoneService>();
		builder.Services.AddScoped<IWorkAreaService, WorkAreaService>();
		builder.Services.AddScoped<IWorkAreaDetailService, WorkAreaDetailService>();
        builder.Services.AddScoped<ISlaService, SlaService>();

        // Dependency Injection for Azure Blob Storage Service
        builder.Services.AddScoped<IFileStorageService, AzureBlobStorageService>();

        // ENUM -> STRING JSON
        builder.Services.Configure<JsonOptions>(options =>
        {
            options.JsonSerializerOptions.Converters
                .Add(new JsonStringEnumConverter());
        });
    }
}
