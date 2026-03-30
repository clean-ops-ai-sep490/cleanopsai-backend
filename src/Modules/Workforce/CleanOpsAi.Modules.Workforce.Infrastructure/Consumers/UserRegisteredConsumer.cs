using CleanOpsAi.BuildingBlocks.Infrastructure.Events;
using CleanOpsAi.Modules.Workforce.Application.Dtos.Workers;
using CleanOpsAi.Modules.Workforce.Application.Interfaces;
using MassTransit;

namespace CleanOpsAi.Modules.Workforce.Infrastructure.Consumers
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
            Console.WriteLine("=== EVENT RECEIVED ===");
            Console.WriteLine($"UserId: {context.Message.UserId}");
            Console.WriteLine($"Role: {context.Message.Role}");

            try
            {
                var message = context.Message;

                if (!string.Equals(message.Role, "Worker", StringComparison.OrdinalIgnoreCase)
                    && message.Role != "1")
                {
                    Console.WriteLine("=== ROLE KHÔNG PHẢI WORKER, BỎ QUA ===");
                    return;
                }

                await _workerService.CreateAsync(new WorkerCreateRequest
                {
                    UserId = Guid.Parse(message.UserId),
                    FullName = message.FullName,
                    AvatarUrl = message.AvatarUrl ?? "https://default-avatar.com/avatar.png"
                });

                Console.WriteLine("=== WORKER CREATED SUCCESSFULLY ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== ERROR: {ex.Message} ===");
                Console.WriteLine(ex.StackTrace);
                throw; // để MassTransit retry và log lỗi
            }
        }
    }
}