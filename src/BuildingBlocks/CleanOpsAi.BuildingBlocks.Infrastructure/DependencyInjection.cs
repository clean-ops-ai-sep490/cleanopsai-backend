using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.BuildingBlocks.Infrastructure;
using CleanOpsAi.BuildingBlocks.Infrastructure.Configs;
using CleanOpsAi.BuildingBlocks.Infrastructure.Services;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection;
public static class DependencyInjection 
{
	public static void InfrastructureBuildingBlocks(this IHostApplicationBuilder builder)
	{

		builder.Services.Configure<FrontendSettings>(
		builder.Configuration.GetSection("Frontend"));
		builder.Services.Configure<EmailSettings>(
		builder.Configuration.GetSection("EmailSettings"));
		builder.Services.AddScoped<IEmailService, EmailService>();

		builder.Services.AddScoped<IDateTimeProvider, DateTimeProvider>();

		builder.Services.AddScoped<IIdGenerator, Uuid7Generator>();

		builder.Services.AddScoped<IUserContext, UserContext>();

	}

}
