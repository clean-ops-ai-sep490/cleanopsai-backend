using AutoMapper;
using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Exceptions;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.BuildingBlocks.Domain.Dtos.Notifications;
using CleanOpsAi.Modules.QualityControl.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.QualityControl.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.QualityControl.Application.Common.Mappings;
using CleanOpsAi.Modules.QualityControl.Application.DTOs.Request;
using CleanOpsAi.Modules.QualityControl.Application.DTOs.Response;
using CleanOpsAi.Modules.QualityControl.Domain.Entities;

namespace CleanOpsAi.Modules.QualityControl.Application.Services
{
	public class NotificationRecipientService : INotificationRecipientService
	{
		private readonly INotificationRecipientRepository _repo;
		private readonly IMapper _mapper;
		private readonly IUserContext _userContext;
		private readonly IIdGenerator _idGenerator;
		private readonly IDateTimeProvider _dateTimeProvider;


		public NotificationRecipientService(
			INotificationRecipientRepository notificationRecipient,
			IMapper mapper, 
			IUserContext userContext, 
			IIdGenerator idGenerator, 
			IDateTimeProvider dateTimeProvider)
		{
			_repo = notificationRecipient;
			_mapper = mapper;
			_userContext = userContext;
			_idGenerator = idGenerator;
			_dateTimeProvider = dateTimeProvider;
		}

		public async Task<NotificationRecipientDto?> Create(NotificationRecipientCreateDto request, CancellationToken ct = default)
		{
			var entity = _mapper.Map<NotificationRecipient>(request);
			entity.Id = _idGenerator.Generate();
			entity.Created = _dateTimeProvider.UtcNow;
			entity.CreatedBy = _userContext.UserId.ToString();

			await _repo.InsertAsync(entity, ct);
			await _repo.SaveChangesAsync(ct);

			return _mapper.Map<NotificationRecipientDto>(entity);
		}

		public async Task<NotificationDetailDto> GetDetailAsync(Guid notificationId, Guid? workerId, CancellationToken ct = default)
		{
			if (notificationId == Guid.Empty)
				throw new BadRequestException("NotificationId cannot be empty.");

			var recipientType = RecipientTypeMapper.FromRole(_userContext.Role);
			Guid? recipientId =
				recipientType == RecipientTypeEnum.Worker
					? workerId ?? throw new BadRequestException("WorkerId is required")
				: recipientType == RecipientTypeEnum.Supervisor
					? _userContext.UserId
				: null;

			var recipient = await _repo.GetDetailAsync(notificationId, recipientId, recipientType, ct);

			if (recipient is null)
				throw new NotFoundException($"Notification with id {notificationId} not found.");

			return _mapper.Map<NotificationDetailDto>(recipient);
		}

		public async Task<(PaginatedResult<NotificationListItemDto> Page, int UnreadCount)> GetPagedByRecipientAsync(PaginationRequest paginationRequest, bool? isRead, Guid? workerId, CancellationToken ct = default)
		{

			var recipientType = RecipientTypeMapper.FromRole(_userContext.Role);

			var recipientId = recipientType == RecipientTypeEnum.Worker
				? workerId ?? throw new BadRequestException("WorkerId is required for worker role")
				: _userContext.UserId;


			var (page, unreadCount) = await _repo.GetPagedByRecipientAsync(
				recipientId,
				recipientType,
				paginationRequest,
				isRead,
				ct);

			var mappedContent = _mapper.Map<List<NotificationListItemDto>>(page.Content);

			var mappedPage = new PaginatedResult<NotificationListItemDto>(
					page.PageNumber,
					page.PageSize,
					page.TotalElements,
					mappedContent);

			return (mappedPage, unreadCount);
		}

		public Task<int> MarkAllAsReadAsync(Guid? workerId, CancellationToken ct = default)
		{
			var recipientType = RecipientTypeMapper.FromRole(_userContext.Role);
			Guid? recipientId =
				recipientType == RecipientTypeEnum.Worker
					? workerId ?? throw new BadRequestException("WorkerId is required")
				: recipientType == RecipientTypeEnum.Supervisor
					? _userContext.UserId
				: null;

			return _repo.MarkAllAsReadAsync(recipientId, recipientType, ct);
		}

		public Task<bool> MarkAsReadAsync(Guid notificationId, Guid? workerId, CancellationToken ct = default)
		{
			var recipientType = RecipientTypeMapper.FromRole(_userContext.Role);
			Guid? recipientId =
			recipientType == RecipientTypeEnum.Worker
				? workerId ?? throw new BadRequestException("WorkerId is required")
			: recipientType == RecipientTypeEnum.Supervisor
				? _userContext.UserId
			: null;

			return _repo.MarkAsReadAsync(notificationId, recipientId, recipientType, ct);
		}
	}
}
