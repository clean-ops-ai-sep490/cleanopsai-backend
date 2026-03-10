using CleanOpsAi.Modules.TaskOperations.Application.Configurations;
using CleanOpsAi.Modules.TaskOperations.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection;
public static class DependencyInjection
{
	public static void InfrastructureTaskOperationsModule(this IHostApplicationBuilder builder)
	{
		builder.Services.AddSingleton(TimeProvider.System);

		builder.Services.AddDbContext<TaskOperationsDbContext>(options =>
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

		//test
	}
}
