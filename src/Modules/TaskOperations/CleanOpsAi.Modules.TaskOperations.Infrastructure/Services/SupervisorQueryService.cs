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
        private readonly IRequestClient<GetSupervisorNameByUserIdRequest> _supervisorNameClient;

        public SupervisorQueryService(
            IRequestClient<GetSupervisorByWorkerAndWorkAreaRequest> client, IRequestClient<GetSupervisorNameByUserIdRequest> supervisorNameClient)
        {
            _client = client;
            _supervisorNameClient = supervisorNameClient;
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

            Console.WriteLine($"DEBUG response - Found: {response.Message.Found}, SupervisorId: {response.Message.SupervisorId}");

            return response.Message.Found
                ? response.Message.SupervisorId
                : null;
        }

        public async Task<string?> GetSupervisorNameAsync(Guid userId, CancellationToken ct = default)
        {
            var response = await _supervisorNameClient.GetResponse<GetSupervisorNameByUserIdResponse>(
                new GetSupervisorNameByUserIdRequest { UserId = userId }, ct);

            return response.Message.Found ? response.Message.FullName : null;
        }
    }
}
