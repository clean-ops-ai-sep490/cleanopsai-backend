namespace CleanOpsAi.BuildingBlocks.Application.Pagination
{
	public record PaginationRequest(int PageNumber = 1, int PageSize = 10);
}
