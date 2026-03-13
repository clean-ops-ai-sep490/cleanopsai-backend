using CleanOpsAi.Modules.ClientManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.ClientManagement.Application.Interfaces
{
    public interface ISlaShiftRepository
    {
        Task<SlaShift?> GetByIdAsync(Guid id);

        Task<List<SlaShift>> GetBySlaIdAsync(Guid slaId);

        Task<(List<SlaShift>, int)> GetAllPaginationAsync(int pageNumber, int pageSize);

        Task<int> CreateAsync(SlaShift entity);

        Task<int> UpdateAsync(SlaShift entity);

        Task<int> DeleteAsync(Guid id);
    }
}
