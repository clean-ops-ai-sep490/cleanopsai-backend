using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.BuildingBlocks.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddMessageBroker(
            this IServiceCollection services,
            IConfiguration configuration,
            params Assembly[] consumerAssemblies)
        {
            services.AddMassTransit(x =>
            {
                // scan consumer từ module
                x.AddConsumers(consumerAssemblies);

                x.UsingRabbitMq((context, cfg) =>
                {
                    var broker = configuration.GetSection("MessageBroker");

                    cfg.Host(broker["Host"], "/", h =>
                    {
                        h.Username(broker["Username"]);
                        h.Password(broker["Password"]);
                    });

                    cfg.ConfigureEndpoints(context);
                });
            });

            return services;
        }
    }
}
