using CleanOpsAi.Modules.Workforce.Application.Dtos;
using CleanOpsAi.Modules.Workforce.Application.Dtos.Certifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.Workforce.Application.Interfaces
{
    public interface ICertificationService
    {
        Task<List<CertificationResponse>> GetByIdAsync(Guid id);

        Task<List<CertificationResponse>> GetAllAsync();

        Task<PagedResponse<CertificationResponse>> GetAllPaginationAsync(int pageNumber, int pageSize);

        Task<CertificationResponse> CreateAsync(CertificationCreateRequest request);

        Task<CertificationResponse> UpdateAsync(Guid id, CertificationUpdateRequest request);

        Task<int> DeleteAsync(Guid id);

        Task<List<string>> GetAllCategoriesAsync();

        Task<List<WorkerCertificationResponse>> GetCertificationsByWorkerIdAsync(Guid workerId);

        Task<List<CertificationResponse>> GetByCategoryAsync(string category);

        Task<WorkerSkillCertificationResponse> GetByIdsAsync(List<Guid> skillIds, List<Guid> certificationIds);
    }
}
