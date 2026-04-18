namespace CleanOpsAi.BuildingBlocks.Application.Pagination
{
	public class PaginatedResult<TEntity>
	where TEntity : class
	{
		public int PageNumber { get; }
		public int PageSize { get; }
		public int TotalElements { get; }

		public int TotalPages => (int)Math.Ceiling((double)TotalElements / PageSize);

		public bool HasPreviousPage => PageNumber > 1;
		public bool HasNextPage => PageNumber < TotalPages;

		public IReadOnlyList<TEntity> Content { get; }

		public PaginatedResult(
			int pageNumber,
			int pageSize,
			int totalElements,
			IEnumerable<TEntity> content)
		{
			if (pageNumber <= 0) throw new ArgumentException("PageNumber must be > 0");
			if (pageSize <= 0) throw new ArgumentException("PageSize must be > 0");

			PageNumber = pageNumber;
			PageSize = pageSize;
			TotalElements = totalElements;
			Content = content.ToList();
		}
	}
}
