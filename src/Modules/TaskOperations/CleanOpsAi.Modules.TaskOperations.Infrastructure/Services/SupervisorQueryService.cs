using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request;
using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Response;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.TaskOperations.Infrastructure.Services
{
    public class SupervisorQueryService : ISupervisorQueryService
    {
        private readonly IRequestClient<GetSupervisorByWorkerAndWorkAreaRequest> _client;

        public SupervisorQueryService(
            IRequestClient<GetSupervisorByWorkerAndWorkAreaRequest> client)
        {
            _client = client;
        }

        public async Task<Guid?> GetSupervisorIdAsync(
            Guid workAreaId,
            Guid workerId,
            CancellationToken ct = default)
        {
            var response = await _client.GetResponse<GetSupervisorByWorkerAndWorkAreaResponse>(
                new GetSupervisorByWorkerAndWorkAreaRequest
                {
                    WorkAreaId = workAreaId,
                    WorkerId = workerId
                }, ct);

            return response.Message.Found
                ? response.Message.SupervisorId
                : null;
        }
    }
}
