using CleanOpsAi.Modules.Workforce.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.Workforce.Application.Interfaces
{
    public interface IEquipmentRepository
    {
        Task<Equipment?> GetByIdAsync(Guid id);

        Task<List<Equipment>> GetAllAsync();

        Task<(List<Equipment> Items, int TotalCount)> GetAllPaginationAsync(int pageNumber, int pageSize);

        Task<int> CreateAsync(Equipment equipment);

        Task<int> UpdateAsync(Equipment equipment);

        Task<int> DeleteAsync(Guid id);

        Task<(List<Equipment> Items, int TotalCount)> SearchPaginationAsync(string? keyword, int pageNumber, int pageSize);

        Task<List<Equipment>> GetByIdsAsync(List<Guid> ids);
    }
}
