using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection; 
using System.Reflection; 

namespace CleanOpsAi.BuildingBlocks.Infrastructure.Extensions
{
	public static class MassTransitExtensions
	{
		public static IServiceCollection AddMessageBroker(
			this IServiceCollection services,
			IConfiguration configuration,
			params Assembly[] consumerAssemblies) 
		{
			services.AddMassTransit(x =>
			{ 
				foreach (var assembly in consumerAssemblies)
				{
					x.AddConsumers(assembly);
				}

				x.UsingRabbitMq((ctx, cfg) =>
				{
					cfg.Host(configuration["MessageBroker:Host"], h =>
					{
						h.Username(configuration["MessageBroker:Username"]!);
						h.Password(configuration["MessageBroker:Password"]!);
					});

					cfg.ConfigureEndpoints(ctx);
				});
			});

			return services;
		}
	}
}
