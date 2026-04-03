using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request;
using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Response;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services;
using MassTransit; 

namespace CleanOpsAi.Modules.TaskOperations.Infrastructure.Services.Features.Queries
{
    public class WorkAreaQueryService : IWorkAreaQueryService
    {
        private readonly IRequestClient<GetWorkAreaByIdRequest> _client;

        public WorkAreaQueryService(IRequestClient<GetWorkAreaByIdRequest> client)
        {
            _client = client;
        }

        public async Task<string?> GetWorkAreaNameAsync(Guid workAreaId, CancellationToken ct = default)
        {
            var response = await _client.GetResponse<GetWorkAreaByIdResponse>(
                new GetWorkAreaByIdRequest { WorkAreaId = workAreaId }, ct);

            return response.Message.Found ? response.Message.WorkAreaName : null;
        }
    }
}
