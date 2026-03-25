using AutoMapper;
using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.Modules.QualityControl.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.QualityControl.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.QualityControl.Application.DTOs.Response;

namespace CleanOpsAi.Modules.QualityControl.Application.Services
{
	public class NotificationRecipientService : INotificationRecipientService
	{
		private readonly INotificationRecipientRepository _repo;
		private readonly IMapper _mapper;
		private readonly IUserContext _userContext;

		public NotificationRecipientService(INotificationRecipientRepository notificationRecipient,
			IMapper mapper, IUserContext userContext)
		{
			_repo = notificationRecipient;
			_mapper = mapper;
			_userContext = userContext;
		}

		public async Task<NotificationDetailDto?> GetDetailAsync(Guid notificationId, CancellationToken ct = default)
		{
			var recipient = await _repo.GetDetailAsync(notificationId, _userContext.UserId, ct);
			return recipient is null ? null : _mapper.Map<NotificationDetailDto>(recipient);
		}

		public async Task<(PaginatedResult<NotificationListItemDto> Page, int UnreadCount)> GetPagedByRecipientAsync(PaginationRequest paginationRequest, bool? isRead, CancellationToken ct = default)
		{
			var (page, unreadCount) = await _repo.GetPagedByRecipientAsync(_userContext.UserId, paginationRequest, isRead, ct);

			var mappedContent = _mapper.Map<List<NotificationListItemDto>>(page.Content);

			var mappedPage = new PaginatedResult<NotificationListItemDto>(
					page.PageNumber,
					page.PageSize,
					page.TotalElements,
					mappedContent);

			return (mappedPage, unreadCount);
		}

		public Task<int> MarkAllAsReadAsync(CancellationToken ct = default)
		 => _repo.MarkAllAsReadAsync(_userContext.UserId, ct);

		public Task<bool> MarkAsReadAsync(Guid notificationId, CancellationToken ct = default)
		=> _repo.MarkAsReadAsync(notificationId, _userContext.UserId, ct);
	}
}
