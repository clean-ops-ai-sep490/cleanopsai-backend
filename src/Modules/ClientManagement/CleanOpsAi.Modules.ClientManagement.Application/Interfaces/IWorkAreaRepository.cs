using CleanOpsAi.Modules.ClientManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.ClientManagement.Application.Interfaces
{
    public interface IWorkAreaRepository
    {
        Task<WorkArea?> GetByIdAsync(Guid id);
        Task<List<WorkArea>> GetAllAsync();
        Task<(List<WorkArea> Items, int TotalCount)> GetAllPaginationAsync(int pageNumber, int pageSize);
        Task<(List<WorkArea> Items, int TotalCount)> GetByZoneIdPaginationAsync(Guid zoneId, int pageNumber, int pageSize);
        Task<int> CreateAsync(WorkArea workArea);
        Task<int> UpdateAsync(WorkArea workArea);
        Task<int> DeleteAsync(Guid id);
    }
}
