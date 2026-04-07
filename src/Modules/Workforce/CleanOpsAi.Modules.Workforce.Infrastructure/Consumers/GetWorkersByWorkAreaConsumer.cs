using CleanOpsAi.Modules.Workforce.Application.Interfaces;
using CleanOpsAi.Modules.Workforce.Application.Dtos.Workers;
using MassTransit;

using WorkerRequest = CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request.GetWorkersByWorkAreaRequest;
using WorkerResponse = CleanOpsAi.BuildingBlocks.Infrastructure.Events.Response.GetWorkersByWorkAreaResponse;
using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request;
using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Response;

namespace CleanOpsAi.Modules.Workforce.Infrastructure.Consumers
{
    public class GetWorkersByWorkAreaConsumer : IConsumer<GetWorkersByWorkAreaRequest>
    {
        private readonly IWorkAreaSupervisorService _service;

        public GetWorkersByWorkAreaConsumer(IWorkAreaSupervisorService service)
        {
            _service = service;
        }

        public async Task Consume(ConsumeContext<GetWorkersByWorkAreaRequest> context)
        {
            try
            {
                Console.WriteLine(" HIT GetWorkersByWorkAreaConsumer");

                var items = await _service.GetByWorkAreaIdAsync(context.Message.WorkAreaId);

                var workers = items
                    .Where(x => x.WorkerId != Guid.Empty)
                    .GroupBy(x => x.WorkerId)
                    .Select(g => g.First())
                    .Select(x => new WorkerDto
                    {
                        Id = x.WorkerId.Value,
                        FullName = x.WorkerName ?? "Unknown"
                    })
                    .ToList();

                // QUAN TRỌNG: dùng đúng type response (KHÔNG alias)
                await context.RespondAsync(new GetWorkersByWorkAreaResponse
                {
                    Workers = workers
                });
            }
            catch (Exception ex)
            {
                //  BẮT LỖI ĐỂ BIẾT NÓ CHẾT Ở ĐÂU
                Console.WriteLine(" ERROR CONSUMER: " + ex.ToString());
                throw;
            }
        }
    }
}