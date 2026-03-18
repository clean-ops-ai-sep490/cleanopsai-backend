using CleanOpsAi.BuildingBlocks.Infrastructure.Events;
using CleanOpsAi.Modules.Workforce.Application.Dtos.Workers;
using CleanOpsAi.Modules.Workforce.Application.Interfaces;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.Workforce.Application.Consumers
{
    public class UserRegisteredConsumer : IConsumer<UserRegisteredIntegrationEvent>
    {
        private readonly IWorkerService _workerService;

        public UserRegisteredConsumer(IWorkerService workerService)
        {
            _workerService = workerService;
        }

        public async Task Consume(ConsumeContext<UserRegisteredIntegrationEvent> context)
        {
            Console.WriteLine("EVENT RECEIVED");
            var message = context.Message;

            if (message.Role != "Worker")
                return;

            await _workerService.CreateAsync(new WorkerCreateRequest
            {
                UserId = message.UserId,
                FullName = message.FullName,
                AvatarUrl = context.Message.AvatarUrl ?? "https://default-avatar.com/avatar.png"
            });
        }
    }
}
