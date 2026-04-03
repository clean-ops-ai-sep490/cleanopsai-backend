using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request;
using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Response;
using CleanOpsAi.Modules.ClientManagement.Application.Interfaces;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.ClientManagement.Infrastructure.Consumers
{
    public class GetWorkAreaByIdConsumer : IConsumer<GetWorkAreaByIdRequest>
    {
        private readonly IWorkAreaRepository _repository;

        public GetWorkAreaByIdConsumer(IWorkAreaRepository repository)
        {
            _repository = repository;
        }

        public async Task Consume(ConsumeContext<GetWorkAreaByIdRequest> context)
        {
            var workArea = await _repository.GetByIdAsync(context.Message.WorkAreaId);

            if (workArea == null)
            {
                await context.RespondAsync(new GetWorkAreaByIdResponse { Found = false });
                return;
            }

            await context.RespondAsync(new GetWorkAreaByIdResponse
            {
                Found = true,
                WorkAreaId = workArea.Id,
                WorkAreaName = workArea.Name
            });
        }
    }
}
