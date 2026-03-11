using CleanOpsAi.Modules.ClientManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.ClientManagement.Application.Interfaces
{
    public interface IZoneRepository
    {
        Task<Zone?> GetByIdAsync(Guid id);
        Task<List<Zone>> GetAllAsync();
        Task<(List<Zone> Items, int TotalCount)> GetAllPaginationAsync(int pageNumber, int pageSize);
        Task<(List<Zone> Items, int TotalCount)> GetByLocationIdPaginationAsync(Guid locationId, int pageNumber, int pageSize);
        Task<int> CreateAsync(Zone zone);
        Task<int> UpdateAsync(Zone zone);
        Task<int> DeleteAsync(Guid id);
    }
}
