using CleanOpsAi.Modules.Workforce.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.Workforce.Application.Interfaces
{
    public interface ICertificationRepository
    {
        Task<Certification?> GetByIdAsync(Guid id);

        Task<List<Certification>> GetAllAsync();

        Task<(List<Certification> Items, int TotalCount)> GetAllPaginationAsync(int pageNumber, int pageSize);

        Task<int> CreateAsync(Certification certification);

        Task<int> UpdateAsync(Certification certification);

        Task<int> DeleteAsync(Guid id);
    }
}
