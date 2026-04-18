using CleanOpsAi.BuildingBlocks.Application.Pagination;
using Microsoft.EntityFrameworkCore; 

namespace CleanOpsAi.BuildingBlocks.Infrastructure.Extensions
{
	public static class PaginationExtensions
	{
		public static async Task<PaginatedResult<T>> ToPaginatedResultAsync<T>(
			this IQueryable<T> query,
			PaginationRequest request,
			CancellationToken ct = default)
			where T : class
		{
			var totalElements = await query.CountAsync(ct);

			var items = await query
				.Skip((request.PageNumber - 1) * request.PageSize)
				.Take(request.PageSize)
				.ToListAsync(ct);

			return new PaginatedResult<T>(
				request.PageNumber,
				request.PageSize,
				totalElements,
				items);
		}
	}
}
