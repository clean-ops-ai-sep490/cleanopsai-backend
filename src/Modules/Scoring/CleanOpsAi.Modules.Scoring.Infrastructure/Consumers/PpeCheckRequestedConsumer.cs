using CleanOpsAi.BuildingBlocks.Application.Interfaces.Messaging;
using CleanOpsAi.BuildingBlocks.Infrastructure.Events;
using CleanOpsAi.Modules.Scoring.Application.Common.Interfaces.Services;
using MassTransit; 
namespace CleanOpsAi.Modules.Scoring.Infrastructure.Consumers
{
	public class PpeCheckRequestedConsumer : IConsumer<PpeCheckRequestedEvent>
	{
		private readonly IScoringInferenceClient _inferenceClient;
		private readonly IEventBus _eventBus;

		public PpeCheckRequestedConsumer(
			IScoringInferenceClient inferenceClient,
			IEventBus eventBus)
		{
			_inferenceClient = inferenceClient;
			_eventBus = eventBus;
		}

		public async Task Consume(ConsumeContext<PpeCheckRequestedEvent> context)
		{
			var evt = context.Message;
			try
			{
				var result = await _inferenceClient.EvaluatePpeAsync(
					evt.ImageUrls,
					evt.RequiredObjects,
					evt.MinConfidence,
					context.CancellationToken);

				await _eventBus.PublishAsync(new PpeCheckCompletedEvent
				{
					TaskStepExecutionId = evt.TaskStepExecutionId,
					Status = result.Status ?? "ERROR",
					Message = result.Message,
					ImageUrls = evt.ImageUrls.ToList(),          
					RequiredObjects = evt.RequiredObjects.ToList(),
					DetectedItems = result.DetectedItems
						.Select(x => new PpeDetectedItem
						{
							Name = x.Name,
							Confidence = x.Confidence,
							ImageIndex = x.ImageIndex,
						}).ToList(),
					MissingItems = result.MissingItems ?? [],
					FailedImages = result.FailedImages
						.Select(x => new PpeFailedImage
						{
							ImageUrl = x.ImageUrl,
							ImageIndex = x.ImageIndex,
							Error = x.Error,
						}).ToList(),
				}, context.CancellationToken);
			}
			catch (Exception ex)
			{
				await _eventBus.PublishAsync(new PpeCheckCompletedEvent
				{
					TaskStepExecutionId = evt.TaskStepExecutionId,
					Status = "ERROR",
					Message = ex.Message,
				}, context.CancellationToken);
			}
		}
	}
}
