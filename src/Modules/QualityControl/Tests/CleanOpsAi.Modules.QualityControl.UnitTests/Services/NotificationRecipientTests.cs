using AutoMapper;
using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Exceptions;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.BuildingBlocks.Domain.Dtos.Notifications;
using CleanOpsAi.Modules.QualityControl.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.QualityControl.Application.DTOs.Response;
using CleanOpsAi.Modules.QualityControl.Application.Services;
using CleanOpsAi.Modules.QualityControl.Domain.Entities;
using NSubstitute;

namespace CleanOpsAi.Modules.QualityControl.UnitTests.Services
{
	public class NotificationRecipientTests
	{
		private readonly INotificationRecipientRepository _repo;
		private readonly IMapper _mapper;
		private readonly IUserContext _userContext;
		private readonly NotificationRecipientService _service;
		private readonly IIdGenerator _idGenerator;
		private readonly IDateTimeProvider _dateTimeProvider;

		public NotificationRecipientTests()
		{
			_repo = Substitute.For<INotificationRecipientRepository>();
			_mapper = Substitute.For<IMapper>();
			_userContext = Substitute.For<IUserContext>();
			_idGenerator = Substitute.For<IIdGenerator>();
			_dateTimeProvider = Substitute.For<IDateTimeProvider>();

			_service = new NotificationRecipientService(
				_repo,
				_mapper,
				_userContext,
				_idGenerator,
				_dateTimeProvider);
		}

		// ---------------------------------------------------------------
		// Helpers
		// ---------------------------------------------------------------

		private void StubGetDetail(Guid notificationId, Guid? recipientId, RecipientTypeEnum recipientType, NotificationRecipient? returnValue)
		{
			_repo.GetDetailAsync(
					notificationId,
					recipientId,
					recipientType,
					Arg.Any<CancellationToken>())
				.Returns(returnValue);
		}

		private void StubGetPaged(
			Guid recipientId,
			RecipientTypeEnum recipientType,
			bool? isRead,
			PaginatedResult<NotificationRecipient> page,
			int unreadCount)
		{
			_repo.GetPagedByRecipientAsync(
					recipientId,
					recipientType,
					Arg.Any<PaginationRequest>(),
					isRead,
					Arg.Any<CancellationToken>())
				.Returns((page, unreadCount));
		}

		// ===============================================================
		// GET DETAIL
		// ===============================================================

		[Fact]
		public async Task GetDetailAsync_WhenNotificationExists_WithWorkerRole_ReturnsMappedDto()
		{
			var notificationId = Guid.NewGuid();
			var userId = Guid.NewGuid();
			var workerId = Guid.NewGuid();
			var recipient = new NotificationRecipient { NotificationId = notificationId };
			var expectedDto = new NotificationDetailDto { NotificationId = notificationId };

			_userContext.UserId.Returns(userId);
			_userContext.Role.Returns("Worker");

			StubGetDetail(notificationId, workerId, RecipientTypeEnum.Worker, recipient);
			_mapper.Map<NotificationDetailDto>(recipient).Returns(expectedDto);

			var result = await _service.GetDetailAsync(notificationId, workerId);

			Assert.NotNull(result);
			Assert.Equal(expectedDto.NotificationId, result.NotificationId);
		}

		[Fact]
		public async Task GetDetailAsync_WhenNotificationExists_WithSupervisorRole_ReturnsMappedDto()
		{
			var notificationId = Guid.NewGuid();
			var userId = Guid.NewGuid();
			var recipient = new NotificationRecipient { NotificationId = notificationId };
			var expectedDto = new NotificationDetailDto { NotificationId = notificationId };

			_userContext.UserId.Returns(userId);
			_userContext.Role.Returns("Supervisor");

			StubGetDetail(notificationId, userId, RecipientTypeEnum.Supervisor, recipient);
			_mapper.Map<NotificationDetailDto>(recipient).Returns(expectedDto);

			var result = await _service.GetDetailAsync(notificationId, workerId: null);

			Assert.NotNull(result);
			Assert.Equal(expectedDto.NotificationId, result.NotificationId);
		}

		[Fact]
		public async Task GetDetailAsync_WhenNotificationNotFound_ThrowsNotFoundException()
		{
			var notificationId = Guid.NewGuid();
			var userId = Guid.NewGuid();
			var workerId = Guid.NewGuid();

			_userContext.UserId.Returns(userId);
			_userContext.Role.Returns("Worker");

			StubGetDetail(notificationId, workerId, RecipientTypeEnum.Worker, null);

			await Assert.ThrowsAsync<NotFoundException>(() => _service.GetDetailAsync(notificationId, workerId));
		}

		[Fact]
		public async Task GetDetailAsync_WhenNotificationIdIsEmpty_ThrowsBadRequestException()
		{
			await Assert.ThrowsAsync<BadRequestException>(() => _service.GetDetailAsync(Guid.Empty, workerId: null));
		}

		[Fact]
		public async Task GetDetailAsync_WithWorkerRole_NoWorkerId_ThrowsBadRequestException()
		{
			_userContext.UserId.Returns(Guid.NewGuid());
			_userContext.Role.Returns("Worker");

			await Assert.ThrowsAsync<BadRequestException>(() =>
				_service.GetDetailAsync(Guid.NewGuid(), workerId: null));
		}

		// ===============================================================
		// GET PAGED
		// ===============================================================

		[Fact]
		public async Task GetPagedByRecipientAsync_WithWorkerRole_ReturnsPageAndUnreadCount()
		{
			var userId = Guid.NewGuid();
			var workerId = Guid.NewGuid();
			var request = new PaginationRequest { PageNumber = 1, PageSize = 10 };
			var recipients = new List<NotificationRecipient>
			{
				new NotificationRecipient(),
				new NotificationRecipient()
			};
			var page = new PaginatedResult<NotificationRecipient>(1, 10, 2, recipients);

			_userContext.UserId.Returns(userId);
			_userContext.Role.Returns("Worker");

			StubGetPaged(workerId, RecipientTypeEnum.Worker, isRead: null, page, unreadCount: 1);
			_mapper.Map<List<NotificationListItemDto>>(Arg.Any<List<NotificationRecipient>>())
				.Returns(new List<NotificationListItemDto> { new(), new() });

			var (resultPage, unreadCount) = await _service.GetPagedByRecipientAsync(request, null, workerId);

			Assert.Equal(2, resultPage.TotalElements);
			Assert.Equal(2, resultPage.Content.Count);
			Assert.Equal(1, unreadCount);
		}

		[Fact]
		public async Task GetPagedByRecipientAsync_WithWorkerRole_NoWorkerId_ThrowsBadRequestException()
		{
			_userContext.UserId.Returns(Guid.NewGuid());
			_userContext.Role.Returns("Worker");

			await Assert.ThrowsAsync<BadRequestException>(() =>
				_service.GetPagedByRecipientAsync(
					new PaginationRequest { PageNumber = 1, PageSize = 10 },
					null,
					workerId: null));
		}

		[Fact]
		public async Task GetPagedByRecipientAsync_WithSupervisorRole_UsesUserIdAsRecipient()
		{
			var userId = Guid.NewGuid();
			var request = new PaginationRequest { PageNumber = 1, PageSize = 10 };
			var page = new PaginatedResult<NotificationRecipient>(1, 10, 1,
				new List<NotificationRecipient> { new NotificationRecipient() });

			_userContext.UserId.Returns(userId);
			_userContext.Role.Returns("Supervisor");

			StubGetPaged(userId, RecipientTypeEnum.Supervisor, isRead: null, page, unreadCount: 0);
			_mapper.Map<List<NotificationListItemDto>>(Arg.Any<List<NotificationRecipient>>())
				.Returns(new List<NotificationListItemDto> { new() });

			var (resultPage, unreadCount) = await _service.GetPagedByRecipientAsync(request, null, workerId: null);

			Assert.Equal(1, resultPage.TotalElements);
			Assert.Equal(0, unreadCount);
		}

		[Fact]
		public async Task GetPagedByRecipientAsync_WithIsReadFilter_ReturnsFilteredPage()
		{
			var userId = Guid.NewGuid();
			var workerId = Guid.NewGuid();
			var request = new PaginationRequest { PageNumber = 1, PageSize = 10 };
			var page = new PaginatedResult<NotificationRecipient>(1, 10, 1,
				new List<NotificationRecipient> { new NotificationRecipient { IsRead = true } });

			_userContext.UserId.Returns(userId);
			_userContext.Role.Returns("Worker");

			StubGetPaged(workerId, RecipientTypeEnum.Worker, isRead: true, page, unreadCount: 0);
			_mapper.Map<List<NotificationListItemDto>>(Arg.Any<List<NotificationRecipient>>())
				.Returns(new List<NotificationListItemDto> { new() });

			var (resultPage, unreadCount) = await _service.GetPagedByRecipientAsync(request, true, workerId);

			Assert.Equal(1, resultPage.TotalElements);
			Assert.Equal(0, unreadCount);
		}

		[Fact]
		public async Task GetPagedByRecipientAsync_WhenNoNotifications_ReturnsEmptyPage()
		{
			var userId = Guid.NewGuid();
			var workerId = Guid.NewGuid();
			var request = new PaginationRequest { PageNumber = 1, PageSize = 10 };
			var emptyPage = new PaginatedResult<NotificationRecipient>(1, 10, 0, new List<NotificationRecipient>());

			_userContext.UserId.Returns(userId);
			_userContext.Role.Returns("Worker");

			StubGetPaged(workerId, RecipientTypeEnum.Worker, isRead: null, emptyPage, unreadCount: 0);
			_mapper.Map<List<NotificationListItemDto>>(Arg.Any<List<NotificationRecipient>>())
				.Returns(new List<NotificationListItemDto>());

			var (resultPage, unreadCount) = await _service.GetPagedByRecipientAsync(request, null, workerId);

			Assert.Equal(0, resultPage.TotalElements);
			Assert.Empty(resultPage.Content);
			Assert.Equal(0, unreadCount);
		}

		// ===============================================================
		// MARK AS READ
		// ===============================================================

		[Fact]
		public async Task MarkAsReadAsync_WithWorkerRole_WhenRecipientExists_ReturnsTrue()
		{
			var notificationId = Guid.NewGuid();
			var userId = Guid.NewGuid();
			var workerId = Guid.NewGuid();

			_userContext.UserId.Returns(userId);
			_userContext.Role.Returns("Worker");

			_repo.MarkAsReadAsync(notificationId, workerId, RecipientTypeEnum.Worker, Arg.Any<CancellationToken>())
				.Returns(true);

			var result = await _service.MarkAsReadAsync(notificationId, workerId);

			Assert.True(result);
		}

		[Fact]
		public async Task MarkAsReadAsync_WithSupervisorRole_UsesUserIdAsRecipient()
		{
			var notificationId = Guid.NewGuid();
			var userId = Guid.NewGuid();

			_userContext.UserId.Returns(userId);
			_userContext.Role.Returns("Supervisor");

			_repo.MarkAsReadAsync(notificationId, userId, RecipientTypeEnum.Supervisor, Arg.Any<CancellationToken>())
				.Returns(true);

			var result = await _service.MarkAsReadAsync(notificationId, workerId: null);

			Assert.True(result);
		}

		[Fact]
		public async Task MarkAsReadAsync_WithWorkerRole_NoWorkerId_ThrowsBadRequestException()
		{
			_userContext.UserId.Returns(Guid.NewGuid());
			_userContext.Role.Returns("Worker");

			await Assert.ThrowsAsync<BadRequestException>(() =>
				_service.MarkAsReadAsync(Guid.NewGuid(), workerId: null));
		}

		[Fact]
		public async Task MarkAsReadAsync_WhenAlreadyReadOrNotFound_ReturnsFalse()
		{
			var notificationId = Guid.NewGuid();
			var userId = Guid.NewGuid();
			var workerId = Guid.NewGuid();

			_userContext.UserId.Returns(userId);
			_userContext.Role.Returns("Worker");

			_repo.MarkAsReadAsync(notificationId, workerId, RecipientTypeEnum.Worker, Arg.Any<CancellationToken>())
				.Returns(false);

			var result = await _service.MarkAsReadAsync(notificationId, workerId);

			Assert.False(result);
		}

		// ===============================================================
		// MARK ALL AS READ
		// ===============================================================

		[Fact]
		public async Task MarkAllAsReadAsync_WithWorkerRole_ReturnsCountOfMarkedNotifications()
		{
			var userId = Guid.NewGuid();
			var workerId = Guid.NewGuid();

			_userContext.UserId.Returns(userId);
			_userContext.Role.Returns("Worker");

			_repo.MarkAllAsReadAsync(workerId, RecipientTypeEnum.Worker, Arg.Any<CancellationToken>())
				.Returns(5);

			var result = await _service.MarkAllAsReadAsync(workerId);

			Assert.Equal(5, result);
		}

		[Fact]
		public async Task MarkAllAsReadAsync_WithSupervisorRole_UsesUserIdAsRecipient()
		{
			var userId = Guid.NewGuid();

			_userContext.UserId.Returns(userId);
			_userContext.Role.Returns("Supervisor");

			_repo.MarkAllAsReadAsync(userId, RecipientTypeEnum.Supervisor, Arg.Any<CancellationToken>())
				.Returns(3);

			var result = await _service.MarkAllAsReadAsync(workerId: null);

			Assert.Equal(3, result);
		}

		[Fact]
		public async Task MarkAllAsReadAsync_WithWorkerRole_NoWorkerId_ThrowsBadRequestException()
		{
			_userContext.UserId.Returns(Guid.NewGuid());
			_userContext.Role.Returns("Worker");

			await Assert.ThrowsAsync<BadRequestException>(() =>
				_service.MarkAllAsReadAsync(workerId: null));
		}

		[Fact]
		public async Task MarkAllAsReadAsync_WhenNothingToMark_ReturnsZero()
		{
			var userId = Guid.NewGuid();
			var workerId = Guid.NewGuid();

			_userContext.UserId.Returns(userId);
			_userContext.Role.Returns("Worker");

			_repo.MarkAllAsReadAsync(workerId, RecipientTypeEnum.Worker, Arg.Any<CancellationToken>())
				.Returns(0);

			var result = await _service.MarkAllAsReadAsync(workerId);

			Assert.Equal(0, result);
		}
	}
}