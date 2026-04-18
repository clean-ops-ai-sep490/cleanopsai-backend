using CleanOpsAi.BuildingBlocks.Infrastructure.Events; 
using CleanOpsAi.Modules.QualityControl.Application.Common.Interfaces.Services;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CleanOpsAi.Modules.QualityControl.Infrastructure.Consumers
{
	public class SendNotificationConsumer : IConsumer<SendNotificationEvent>
	{
		private readonly INotificationService _notificationService;
		private readonly ILogger<SendNotificationConsumer> _logger;

		public SendNotificationConsumer(
			INotificationService notificationService,
			ILogger<SendNotificationConsumer> logger)
		{
			_notificationService = notificationService;
			_logger = logger;
		}

		public async Task Consume(ConsumeContext<SendNotificationEvent> context)
		{
			var message = context.Message;

			try
			{
				await _notificationService.HandleSendNotificationAsync(message, context.CancellationToken);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error processing SendNotificationEvent. Title: {Title}", message.Title);
				throw;  
			}
		}
	}
}
