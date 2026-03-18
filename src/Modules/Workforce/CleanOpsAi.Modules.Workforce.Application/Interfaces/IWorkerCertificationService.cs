using CleanOpsAi.Modules.Workforce.Application.Dtos;
using CleanOpsAi.Modules.Workforce.Application.Dtos.WorkerCertifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.Workforce.Application.Interfaces
{
    public interface IWorkerCertificationService
    {
        Task<WorkerCertificationResponse?> GetByIdAsync(Guid workerId, Guid certificationId);

        Task<List<WorkerCertificationResponse>> GetAllAsync();

        Task<PagedResponse<WorkerCertificationResponse>> GetAllPaginationAsync(int pageNumber, int pageSize);

        Task<WorkerCertificationResponse?> CreateAsync(WorkerCertificationCreateRequest request);

        Task<WorkerCertificationResponse?> UpdateAsync(Guid workerId, Guid certificationId, WorkerCertificationUpdateRequest request);

        Task<int> DeleteAsync(Guid workerId, Guid certificationId);
    }
}
