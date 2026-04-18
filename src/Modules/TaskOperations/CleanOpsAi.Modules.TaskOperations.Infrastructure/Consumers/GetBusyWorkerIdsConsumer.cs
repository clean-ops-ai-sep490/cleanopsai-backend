using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request;
using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Response;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Repositories;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.TaskOperations.Infrastructure.Consumers
{
    public class GetBusyWorkerIdsConsumer : IConsumer<GetBusyWorkerIdsRequest>
    {
        private readonly ITaskAssignmentRepository _taskAssignmentRepository;

        public GetBusyWorkerIdsConsumer(ITaskAssignmentRepository taskAssignmentRepository)
        {
            _taskAssignmentRepository = taskAssignmentRepository;
        }

        public async Task Consume(ConsumeContext<GetBusyWorkerIdsRequest> context)
        {
            var msg = context.Message;

            var busyWorkerIds = await _taskAssignmentRepository
                .GetBusyWorkerIdsWithoutAreaAsync(msg.StartAt, msg.EndAt);

            await context.RespondAsync(new GetBusyWorkerIdsResponse
            {
                BusyWorkerIds = busyWorkerIds
            });
        }
    }
}
