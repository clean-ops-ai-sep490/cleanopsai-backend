using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request;
using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Response;
using CleanOpsAi.Modules.Workforce.Application.Interfaces;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.Workforce.Infrastructure.Consumers
{
    public class GetWorkerIdByUserIdConsumer : IConsumer<GetWorkerIdByUserIdRequest>
    {
        private readonly IWorkerService _workerService;

        public GetWorkerIdByUserIdConsumer(IWorkerService workerService)
        {
            _workerService = workerService;
        }

        public async Task Consume(ConsumeContext<GetWorkerIdByUserIdRequest> context)
        {
            var worker = await _workerService.GetByUserIdAsync(context.Message.UserId);

            if (worker == null)
            {
                await context.RespondAsync(new GetWorkerIdByUserIdResponse
                {
                    Found = false
                });
                return;
            }

            await context.RespondAsync(new GetWorkerIdByUserIdResponse
            {
                Found = true,
                WorkerId = worker.Id,
                FullName = worker.FullName
            });
        }
    }
}
