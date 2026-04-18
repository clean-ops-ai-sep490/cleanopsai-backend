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
    public class GetEquipmentsByIdsConsumer : IConsumer<GetEquipmentsByIdsRequest>
    {
        private readonly IEquipmentRepository _repository;

        public GetEquipmentsByIdsConsumer(IEquipmentRepository repository)
        {
            _repository = repository;
        }

        public async Task Consume(ConsumeContext<GetEquipmentsByIdsRequest> context)
        {
            var ids = context.Message.EquipmentIds.Distinct().ToList();

            var equipments = await _repository.GetByIdsAsync(ids);

            var result = equipments.ToDictionary(
                x => x.Id,
                x => x.Name
            );

            await context.RespondAsync(new GetEquipmentsByIdsResponse
            {
                Equipments = result
            });
        }
    }
}
