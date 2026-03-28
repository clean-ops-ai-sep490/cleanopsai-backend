using CleanOpsAi.Modules.Workforce.Application.Dtos;
using CleanOpsAi.Modules.Workforce.Application.Dtos.Workers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.Workforce.Application.Interfaces
{
    public interface IWorkerService
    {
        Task<List<WorkerResponse>> GetByIdAsync(Guid id);

        Task<List<WorkerResponse>> GetByUserIdAsync(string userId);

        Task<List<WorkerResponse>> GetAllAsync();

        Task<PagedResponse<WorkerResponse>> GetAllPaginationAsync(int pageNumber, int pageSize);

        Task<WorkerResponse> CreateAsync(WorkerCreateRequest request);

        Task<WorkerResponse> UpdateAsync(Guid id, WorkerUpdateRequest request);

        Task<int> DeleteAsync(Guid id);

        Task<List<WorkerResponse>> GetInforAsync();

        Task<List<WorkerResponse>> FilterAsync(WorkerFilterRequest request);
    }
}
