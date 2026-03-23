using CleanOpsAi.Modules.Workforce.Application.Dtos;
using CleanOpsAi.Modules.Workforce.Application.Dtos.WorkerGps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.Workforce.Application.Interfaces
{
    public interface IWorkerGpsService
    {
        Task<WorkerGpsResponse?> GetByIdAsync(Guid id);
        Task<List<WorkerGpsResponse>> GetAllAsync();
        Task<PagedResponse<WorkerGpsResponse>> GetAllPaginationAsync(int pageNumber, int pageSize);
        Task<WorkerGpsResponse?> CreateAsync(WorkerGpsCreateRequest request);

        Task<WorkerGpsResponse?> GetLatestByWorkerIdAsync(Guid workerId);
        Task<List<WorkerGpsResponse>> GetLatestByWorkerIdsAsync(List<Guid> workerIds);
        Task<PagedResponse<WorkerGpsResponse>> GetByWorkerIdPaginationAsync(
            Guid workerId, int pageNumber, int pageSize);
    }
}
