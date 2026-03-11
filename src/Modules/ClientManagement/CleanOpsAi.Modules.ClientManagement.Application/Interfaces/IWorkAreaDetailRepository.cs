using CleanOpsAi.Modules.ClientManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.ClientManagement.Application.Interfaces
{
    public interface IWorkAreaDetailRepository
    {
        Task<WorkAreaDetail?> GetByIdAsync(Guid id);
        Task<(List<WorkAreaDetail>, int)> GetAllPaginationAsync(int pageNumber, int pageSize);
        Task<(List<WorkAreaDetail>, int)> GetByWorkAreaIdPaginationAsync(Guid workAreaId, int pageNumber, int pageSize);
        Task<WorkAreaDetail> CreateAsync(WorkAreaDetail entity);
        Task<int> UpdateAsync(WorkAreaDetail entity);
        Task<int> DeleteAsync(WorkAreaDetail entity);
    }
}
