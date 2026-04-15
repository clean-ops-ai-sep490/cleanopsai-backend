using AutoMapper;
using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.Modules.QualityControl.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.QualityControl.Application.Common.Interfaces.Services;
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

		public NotificationRecipientTests()
		{
			_repo = Substitute.For<INotificationRecipientRepository>();
			_mapper = Substitute.For<IMapper>();
			_userContext = Substitute.For<IUserContext>();

			_service = new NotificationRecipientService(
				_repo,
				_mapper,
				_userContext);
		}

		[Fact]
		public async Task GetDetailAsync_WhenNotificationExists_ReturnsMappedDto()
		{
			var notificationId = Guid.NewGuid();
			var userId = Guid.NewGuid();
			var recipient = new NotificationRecipient { NotificationId = notificationId };
			var expectedDto = new NotificationDetailDto { NotificationId = notificationId };

			_userContext.UserId.Returns(userId);
			_repo.GetDetailAsync(notificationId, userId, Arg.Any<CancellationToken>()).Returns(recipient);
			_mapper.Map<NotificationDetailDto>(recipient).Returns(expectedDto);

			var result = await _service.GetDetailAsync(notificationId);

			Assert.NotNull(result);
			Assert.Equal(expectedDto.NotificationId, result.NotificationId);
		}

		[Fact]
		public async Task GetDetailAsync_WhenNotificationNotFound_ReturnsNull()
		{
			var notificationId = Guid.NewGuid();
			var userId = Guid.NewGuid();

			_userContext.UserId.Returns(userId);
			_repo.GetDetailAsync(notificationId, userId, Arg.Any<CancellationToken>()).Returns((NotificationRecipient?)null);

			var result = await _service.GetDetailAsync(notificationId);

			Assert.Null(result);
		}

		[Fact]
		public async Task GetPagedByRecipientAsync_ReturnsPageAndUnreadCount()
		{
			var userId = Guid.NewGuid();
			var request = new PaginationRequest { PageNumber = 1, PageSize = 10 };
			var recipients = new List<NotificationRecipient> { new NotificationRecipient(), new NotificationRecipient() };
			var mappedDtos = new List<NotificationListItemDto> { new NotificationListItemDto(), new NotificationListItemDto() };
			var page = new PaginatedResult<NotificationRecipient>(1, 10, 2, recipients);

			_userContext.UserId.Returns(userId);
			_repo.GetPagedByRecipientAsync(userId, request, null, Arg.Any<CancellationToken>()).Returns((page, 1));
			_mapper.Map<List<NotificationListItemDto>>(Arg.Any<List<NotificationRecipient>>()).Returns(mappedDtos);

			var (resultPage, unreadCount) = await _service.GetPagedByRecipientAsync(request, null);

			Assert.Equal(2, resultPage.TotalElements);
			Assert.Equal(2, resultPage.Content.Count);
			Assert.Equal(1, unreadCount);
		}

		[Fact]
		public async Task GetPagedByRecipientAsync_WithIsReadFilter_ReturnsFilteredPage()
		{
			var userId = Guid.NewGuid();
			var request = new PaginationRequest { PageNumber = 1, PageSize = 10 };
			var recipients = new List<NotificationRecipient> { new NotificationRecipient() };
			var mappedDtos = new List<NotificationListItemDto> { new NotificationListItemDto() };
			var page = new PaginatedResult<NotificationRecipient>(1, 10, 1, recipients);

			_userContext.UserId.Returns(userId);
			_repo.GetPagedByRecipientAsync(userId, request, true, Arg.Any<CancellationToken>()).Returns((page, 0));
			_mapper.Map<List<NotificationListItemDto>>(Arg.Any<List<NotificationRecipient>>()).Returns(mappedDtos);

			var (resultPage, unreadCount) = await _service.GetPagedByRecipientAsync(request, true);

			Assert.Equal(1, resultPage.TotalElements);
			Assert.Equal(0, unreadCount);
		}

		[Fact]
		public async Task MarkAsReadAsync_ReturnsResult()
		{
			var notificationId = Guid.NewGuid();
			var userId = Guid.NewGuid();

			_userContext.UserId.Returns(userId);
			_repo.MarkAsReadAsync(notificationId, userId, Arg.Any<CancellationToken>()).Returns(true);

			var result = await _service.MarkAsReadAsync(notificationId);

			Assert.True(result);
		}

		[Fact]
		public async Task MarkAllAsReadAsync_ReturnsCountOfMarkedNotifications()
		{
			var userId = Guid.NewGuid();
			var expectedCount = 5;

			_userContext.UserId.Returns(userId);
			_repo.MarkAllAsReadAsync(userId, Arg.Any<CancellationToken>()).Returns(expectedCount);

			var result = await _service.MarkAllAsReadAsync();

			Assert.Equal(expectedCount, result);
		}
	}
}
