using CleanOpsAi.Modules.ClientManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.ClientManagement.Application.Interfaces
{
    public interface ISlaRepository
    {
        Task<Sla> GetByIdAsync(Guid id);

        Task<List<Sla>> GetAllAsync();

        Task<(List<Sla> Items, int TotalCount)> GetAllPaginationAsync(int pageNumber, int pageSize);

        Task<(List<Sla> Items, int TotalCount)> FilterPaginationAsync(
            Guid? workAreaId,
            Guid? contractId,
            int pageNumber,
            int pageSize);

        Task<int> CreateAsync(Sla sla);

        Task<int> UpdateAsync(Sla sla);

        Task<int> DeleteAsync(Guid id);
    }
}
