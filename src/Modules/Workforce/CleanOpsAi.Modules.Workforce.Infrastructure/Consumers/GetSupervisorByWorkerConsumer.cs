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
    public class GetSupervisorByWorkerConsumer
        : IConsumer<GetSupervisorByWorkerAndWorkAreaRequest>
    {
        private readonly IWorkAreaSupervisorRepository _repository;

        public GetSupervisorByWorkerConsumer(IWorkAreaSupervisorRepository repository)
        {
            _repository = repository;
        }

        public async Task Consume(ConsumeContext<GetSupervisorByWorkerAndWorkAreaRequest> context)
        {
            var msg = context.Message;

            var entity = await _repository.GetByWorkAreaAndWorkerAsync(
                msg.WorkAreaId, msg.WorkerId);

            if (entity == null)
            {
                await context.RespondAsync(new GetSupervisorByWorkerAndWorkAreaResponse
                {
                    Found = false,
                    SupervisorId = null
                });
                return;
            }

            await context.RespondAsync(new GetSupervisorByWorkerAndWorkAreaResponse
            {
                Found = true,
                SupervisorId = entity.UserId
            });
        }
    }
}
