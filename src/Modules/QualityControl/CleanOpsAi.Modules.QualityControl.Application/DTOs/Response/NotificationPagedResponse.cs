using CleanOpsAi.BuildingBlocks.Application.Pagination; 

namespace CleanOpsAi.Modules.QualityControl.Application.DTOs.Response
{
	public class NotificationPagedResponse : PaginatedResult<NotificationListItemDto>
	{
		public int UnreadCount { get; set; }

		public NotificationPagedResponse(PaginatedResult<NotificationListItemDto> page, int unreadCount)
			: base(page.PageNumber, page.PageSize, page.TotalElements, page.Content)
		{
			UnreadCount = unreadCount;
		}
	}
}
