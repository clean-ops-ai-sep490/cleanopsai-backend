using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request;
using CleanOpsAi.Modules.ClientManagement.Domain.Entities;
using CleanOpsAi.Modules.ClientManagement.Infrastructure.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.ClientManagement.Infrastructure.Consumers
{
    public class WorkAreaConsumer : IConsumer<GetWorkAreasByIdsRequest>
    {
        private readonly ClientManagementDbContext _db;

        public WorkAreaConsumer(ClientManagementDbContext db)
        {
            _db = db;
        }

        public async Task Consume(ConsumeContext<GetWorkAreasByIdsRequest> context)
        {
            var ids = context.Message.WorkAreaIds;

            var items = await _db.Set<WorkArea>()
                .Include(w => w.Zone)
                    .ThenInclude(z => z.Location)
                .Where(w => ids.Contains(w.Id) && !w.IsDeleted)
                .ToListAsync();

            var result = items.Select(entity =>
            {
                var locationName = entity.Zone?.Location?.Name;
                var zoneName = entity.Zone?.Name;
                var workAreaName = entity.Name;

                return new WorkAreaWithLocationDto
                {
                    WorkAreaId = entity.Id,
                    WorkAreaName = workAreaName,
                    ZoneName = zoneName,
                    LocationName = locationName,

                    //  format theo yêu cầu của mày
                    DisplayLocation = string.Join(", ", new[]
                    {
                    locationName,
                    zoneName,
                    workAreaName
                }.Where(x => !string.IsNullOrWhiteSpace(x)))
                };
            }).ToList();

            await context.RespondAsync(new GetWorkAreasByIdsResponse
            {
                Items = result
            });
        }
    }
}
