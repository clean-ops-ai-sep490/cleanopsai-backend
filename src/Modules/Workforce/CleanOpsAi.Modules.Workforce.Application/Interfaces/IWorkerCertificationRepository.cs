using CleanOpsAi.Modules.Workforce.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.Workforce.Application.Interfaces
{
    public interface IWorkerCertificationRepository
    {
        Task<WorkerCertification?> GetByIdAsync(Guid workerId, Guid certificationId);

        Task<List<WorkerCertification>> GetAllAsync();

        Task<(List<WorkerCertification> Items, int TotalCount)> GetAllPaginationAsync(int pageNumber, int pageSize);

        Task<int> CreateAsync(WorkerCertification entity);

        Task<int> UpdateAsync(WorkerCertification entity);

        Task<int> DeleteAsync(Guid workerId, Guid certificationId);
    }
}
