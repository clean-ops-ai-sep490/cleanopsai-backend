using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.BuildingBlocks.Application.Interfaces.Messaging;
using CleanOpsAi.BuildingBlocks.Infrastructure.Events;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.TaskOperations.Application.Services;
using CleanOpsAi.Modules.TaskOperations.Domain.Entities;
using CleanOpsAi.Modules.TaskOperations.Domain.Enums;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace CleanOpsAi.Modules.TaskOperations.UnitTests.Services
{
	public class ComplianceCheckServiceTests
	{
		private readonly IComplianceCheckRepository _complianceRepo;
		private readonly ITaskStepExecutionImageRepository _imageRepo;
		private readonly ITaskStepExecutionRepository _stepExecutionRepo;
		private readonly IEventBus _eventBus;
		private readonly IComplianceNotifier _notifier;
		private readonly ILogger<ComplianceCheckService> _logger;
		private readonly IIdGenerator _idGenerator;
		private readonly IDateTimeProvider _dateTimeProvider;
		private readonly ISupervisorQueryService _supervisorQueryService;
		private readonly IWorkerQueryService _workerQueryService;
		private readonly IUserContext _userContext;
		private readonly ComplianceCheckService _service;

		public ComplianceCheckServiceTests()
		{
			_complianceRepo = Substitute.For<IComplianceCheckRepository>();
			_imageRepo = Substitute.For<ITaskStepExecutionImageRepository>();
			_stepExecutionRepo = Substitute.For<ITaskStepExecutionRepository>();
			_eventBus = Substitute.For<IEventBus>();
			_notifier = Substitute.For<IComplianceNotifier>();
			_logger = Substitute.For<ILogger<ComplianceCheckService>>();
			_idGenerator = Substitute.For<IIdGenerator>();
			_dateTimeProvider = Substitute.For<IDateTimeProvider>();
			_supervisorQueryService = Substitute.For<ISupervisorQueryService>();
			_workerQueryService = Substitute.For<IWorkerQueryService>();
			_userContext = Substitute.For<IUserContext>();

			_service = new ComplianceCheckService(
				_complianceRepo,
				_imageRepo,
				_stepExecutionRepo,
				_eventBus,
				_notifier,
				_logger,
				_idGenerator,
				_dateTimeProvider,
				_supervisorQueryService,
				_workerQueryService,
				_userContext);
		}

		[Fact]
		public async Task InitiateAiCheckAsync_ShouldPublishScoringRequestWithWorkerUserId()
		{
			var taskStepExecutionId = Guid.NewGuid();
			var workerId = Guid.NewGuid();
			var workerUserId = Guid.NewGuid();
			var checkId = Guid.NewGuid();

			_idGenerator.Generate().Returns(checkId);
			_dateTimeProvider.UtcNow.Returns(DateTime.UtcNow);
			_stepExecutionRepo
				.GetByIdDetail(taskStepExecutionId, Arg.Any<CancellationToken>())
				.Returns(new TaskStepExecution
				{
					Id = taskStepExecutionId,
					TaskAssignment = new TaskAssignment
					{
						AssigneeId = workerId
					}
				});
			_imageRepo
				.GetActiveByExecutionIdAndTypeAsync(taskStepExecutionId, ImageType.After, Arg.Any<CancellationToken>())
				.Returns(new List<TaskStepExecutionImage>
				{
					new()
					{
						TaskStepExecutionId = taskStepExecutionId,
						ImageType = ImageType.After,
						ImageUrl = "https://example.com/after.jpg"
					}
				});
			_workerQueryService
				.GetUserIdByWorkerIdAsync(workerId, Arg.Any<CancellationToken>())
				.Returns(workerUserId);
			_eventBus
				.PublishAsync(Arg.Any<AiScoringRequestedEvent>(), Arg.Any<CancellationToken>())
				.Returns(Task.CompletedTask);

			await _service.InitiateAiCheckAsync(taskStepExecutionId);

			await _eventBus.Received(1).PublishAsync(
				Arg.Is<AiScoringRequestedEvent>(evt =>
					evt.ComplianceCheckId == checkId &&
					evt.TaskStepExecutionId == taskStepExecutionId &&
					evt.SubmittedByUserId == workerUserId.ToString() &&
					evt.ImageUrls.Count == 1),
				Arg.Any<CancellationToken>());
		}
	}
}
