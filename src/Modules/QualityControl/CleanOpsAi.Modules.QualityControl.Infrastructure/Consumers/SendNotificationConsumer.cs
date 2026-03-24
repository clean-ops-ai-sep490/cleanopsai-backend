using CleanOpsAi.BuildingBlocks.Infrastructure.Events;
using CleanOpsAi.Modules.QualityControl.Application.Common.Interfaces.Repositories;
using MassTransit;

namespace CleanOpsAi.Modules.QualityControl.Infrastructure.Consumers
{
	public class SendNotificationConsumer : IConsumer<SendNotificationEvent>
	{
		private readonly IFcmTokenRepository _fcmTokenRepository;

		public SendNotificationConsumer()
		{
			
		}
		public Task Consume(ConsumeContext<SendNotificationEvent> context)
		{
			throw new NotImplementedException();
		}
	}
}
