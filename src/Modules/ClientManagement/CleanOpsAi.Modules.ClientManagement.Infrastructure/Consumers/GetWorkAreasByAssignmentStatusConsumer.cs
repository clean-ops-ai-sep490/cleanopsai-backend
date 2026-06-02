using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request;
using CleanOpsAi.Modules.ClientManagement.Domain.Entities;
using CleanOpsAi.Modules.ClientManagement.Infrastructure.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CleanOpsAi.Modules.ClientManagement.Infrastructure.Consumers
{
    public class GetAssignedWorkAreasConsumer : IConsumer<GetAssignedWorkAreasRequest>
    {
        private readonly ClientManagementDbContext _db;

        public GetAssignedWorkAreasConsumer(ClientManagementDbContext db)
        {
            _db = db;
        }

        public async Task Consume(ConsumeContext<GetAssignedWorkAreasRequest> context)
        {
            var response = await GetWorkAreasAsync(
                _db,
                context.Message.AssignedWorkAreaIds,
                includeAssigned: true,
                context.Message.PageNumber,
                context.Message.PageSize,
                context.CancellationToken);

            await context.RespondAsync(response);
        }

        internal static async Task<GetWorkAreasByAssignmentStatusResponse> GetWorkAreasAsync(
            ClientManagementDbContext db,
            List<Guid> assignedWorkAreaIds,
            bool includeAssigned,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            var assignedIds = assignedWorkAreaIds.Distinct().ToList();

            var query = db.Set<WorkArea>()
                .Include(w => w.Zone)
                    .ThenInclude(z => z.Location)
                .Where(w => !w.IsDeleted);

            query = includeAssigned
                ? query.Where(w => assignedIds.Contains(w.Id))
                : query.Where(w => !assignedIds.Contains(w.Id));

            query = query.OrderBy(w => w.Name);

            var totalCount = await query.CountAsync(cancellationToken);

            var entities = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var items = entities.Select(w =>
            {
                var locationName = w.Zone?.Location?.Name;
                var zoneName = w.Zone?.Name;
                var workAreaName = w.Name;

                return new WorkAreaWithLocationDto
                {
                    WorkAreaId = w.Id,
                    WorkAreaName = workAreaName,
                    ZoneName = zoneName,
                    LocationName = locationName,
                    DisplayLocation = string.Join(", ", new[]
                    {
                        locationName,
                        zoneName,
                        workAreaName
                    }.Where(x => !string.IsNullOrWhiteSpace(x)))
                };
            }).ToList();

            return new GetWorkAreasByAssignmentStatusResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalElements = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                Items = items
            };
        }
    }

    public class GetUnassignedWorkAreasConsumer : IConsumer<GetUnassignedWorkAreasRequest>
    {
        private readonly ClientManagementDbContext _db;

        public GetUnassignedWorkAreasConsumer(ClientManagementDbContext db)
        {
            _db = db;
        }

        public async Task Consume(ConsumeContext<GetUnassignedWorkAreasRequest> context)
        {
            var response = await GetAssignedWorkAreasConsumer.GetWorkAreasAsync(
                _db,
                context.Message.AssignedWorkAreaIds,
                includeAssigned: false,
                context.Message.PageNumber,
                context.Message.PageSize,
                context.CancellationToken);

            await context.RespondAsync(response);
        }
    }
}
