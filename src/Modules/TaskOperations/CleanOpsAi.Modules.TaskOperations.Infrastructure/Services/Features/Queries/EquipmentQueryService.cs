using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request;
using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Response;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.TaskOperations.Infrastructure.Services.Features.Queries
{
    public class EquipmentQueryService : IEquipmentQueryService
    {
        private readonly IRequestClient<GetEquipmentsByIdsRequest> _client;

        public EquipmentQueryService(IRequestClient<GetEquipmentsByIdsRequest> client)
        {
            _client = client;
        }

        public async Task<Dictionary<Guid, string>> GetNamesAsync(List<Guid> ids, CancellationToken ct = default)
        {
            if (ids == null || !ids.Any())
                return new Dictionary<Guid, string>();

            var response = await _client.GetResponse<GetEquipmentsByIdsResponse>(
                new GetEquipmentsByIdsRequest
                {
                    EquipmentIds = ids
                }, ct);

            return response.Message.Equipments;
        }
    }
}
