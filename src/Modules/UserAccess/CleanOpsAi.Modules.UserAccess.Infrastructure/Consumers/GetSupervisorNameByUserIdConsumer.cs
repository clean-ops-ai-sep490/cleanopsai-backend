using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request;
using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Response;
using CleanOpsAi.Modules.UserAccess.Domain;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.UserAccess.Infrastructure.Consumers
{
    public class GetSupervisorNameByUserIdConsumer : IConsumer<GetSupervisorNameByUserIdRequest>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public GetSupervisorNameByUserIdConsumer(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task Consume(ConsumeContext<GetSupervisorNameByUserIdRequest> context)
        {
            var user = await _userManager.FindByIdAsync(context.Message.UserId.ToString());

            if (user == null)
            {
                await context.RespondAsync(new GetSupervisorNameByUserIdResponse { Found = false });
                return;
            }

            await context.RespondAsync(new GetSupervisorNameByUserIdResponse
            {
                Found = true,
                FullName = user.FullName // hoặc user.UserName tùy entity
            });
        }
    }
}
