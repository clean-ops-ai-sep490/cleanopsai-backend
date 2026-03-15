using CleanOpsAi.Modules.ServicePlanning.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.ServicePlanning.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.ServicePlanning.Application.Common.Mappings;
using CleanOpsAi.Modules.ServicePlanning.Application.Configurations;
using CleanOpsAi.Modules.ServicePlanning.Application.Services;
using CleanOpsAi.Modules.ServicePlanning.Infrastructure.Data;
using CleanOpsAi.Modules.ServicePlanning.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging; 

namespace Microsoft.Extensions.DependencyInjection;
public static class DependencyInjection
{
	public static void InfrastructureServicePlanningModule(this IHostApplicationBuilder builder)
	{
		builder.Services.AddSingleton(TimeProvider.System);

		builder.Services.AddDbContext<ServicePlanningDbContext>(options =>
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

		builder.Services.AddAutoMapper( cfg => { }, typeof(MappingProfile));

		builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(AssemblyReference.Assembly));

		builder.Services.AddScoped<IStepRepository, StepRepository>();
		builder.Services.AddScoped<ISopRepository, SopRepository>();
		builder.Services.AddScoped<ITaskScheduleRepository, TaskScheduleRepository>();
		builder.Services.AddScoped<ISopStepRepository, SopStepRepository>();


		//Services
		builder.Services.AddScoped<IStepService, StepService>();
		builder.Services.AddScoped<ISopService, SopService>();
		builder.Services.AddScoped<ITaskScheduleService, TaskScheduleService>();
	}
}
