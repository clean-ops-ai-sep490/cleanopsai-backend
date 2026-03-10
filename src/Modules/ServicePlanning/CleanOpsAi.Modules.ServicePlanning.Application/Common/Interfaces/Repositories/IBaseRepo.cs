using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CleanOpsAi.Modules.ServicePlanning.Application.Common.Interfaces.Repositories
{
	public interface IBaseRepo<T, Tkey> where T : class
	{
		ValueTask<EntityEntry<T>> InsertAsync(T entity, CancellationToken cancellationToken = default); 
		Task InsertAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

		Task<bool> UpdateAsync(Tkey id, T entity, CancellationToken cancellationToken = default);
		Task<T?> UpdateTAsync(Tkey id, T entity, CancellationToken cancellationToken = default);


		Task<bool> DeleteAsync(Tkey id, CancellationToken cancellationToken = default);
		Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
		Task<T?> GetByIdAsync(Tkey id, CancellationToken cancellationToken = default); 
		

		Task<T?> GetActiveByIdAsync(Guid id);

		Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
	}
}
