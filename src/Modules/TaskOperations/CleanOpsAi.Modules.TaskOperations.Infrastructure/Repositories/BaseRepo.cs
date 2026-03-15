using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces;
using CleanOpsAi.Modules.TaskOperations.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking; 

namespace CleanOpsAi.Modules.TaskOperations.Infrastructure.Repositories
{
	public class BaseRepo<T, Tkey> : IBaseRepo<T, Tkey> where T : class
	{
		protected readonly TaskOperationsDbContext _context;

		public BaseRepo()
		{
			_context ??= new TaskOperationsDbContext();
		}

		public BaseRepo(TaskOperationsDbContext context)
		{
			_context = context;
		}

		public virtual async ValueTask<EntityEntry<T>> InsertAsync(T entity, CancellationToken cancellationToken = default)
		{
			return await _context.Set<T>().AddAsync(entity, cancellationToken);
		}

		public Task InsertAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
		{
			_context.Set<T>().AddRange(entities);
			return Task.CompletedTask;
		}

		public async Task<bool> DeleteAsync(Tkey id, CancellationToken cancellationToken = default)
		{
			var entity = await _context.Set<T>().FindAsync(new object?[] { id }, cancellationToken);
			if (entity == null) return false;

			_context.Set<T>().Remove(entity); 
			return true;
		}

		public async Task<T?> GetActiveByIdAsync(Guid id, CancellationToken cancellationToken = default)
		{
			var property = typeof(T).GetProperty("Id");
			var isActiveProperty = typeof(T).GetProperty("IsDeleted");

			if (property == null || isActiveProperty == null)
				throw new InvalidOperationException("Entity must have Id and IsDeleted properties");

			return await _context.Set<T>()
				.AsNoTracking()
				.FirstOrDefaultAsync(e =>
					(Guid)property.GetValue(e)! == id &&
					(bool)isActiveProperty.GetValue(e)!,
					cancellationToken);
		}

		public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
		{
			return await _context.Set<T>().ToListAsync(cancellationToken);
		}

		public async Task<T?> GetByIdAsync(Tkey id, CancellationToken cancellationToken = default)
		{
			return await _context.Set<T>().FindAsync(new object[] { id! }, cancellationToken);
		} 

		public async Task<bool> UpdateAsync(Tkey id, T entity, CancellationToken cancellationToken = default)
		{
			var existing = await _context.Set<T>().FindAsync(new object?[] { id }, cancellationToken); 
			if (existing == null) return false;

			_context.Entry(existing).CurrentValues.SetValues(entity);
			return true;
		}

		public async Task<T?> UpdateTAsync(Tkey id, T entity, CancellationToken cancellationToken = default)
		{
			var existing = await _context.Set<T>().FindAsync(new object?[] { id }, cancellationToken); 
			if (existing == null) return null;

			_context.Entry(existing).CurrentValues.SetValues(entity);
			return existing;
		}

		public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
		{
			return await _context.SaveChangesAsync(cancellationToken);
		}
	}
}
