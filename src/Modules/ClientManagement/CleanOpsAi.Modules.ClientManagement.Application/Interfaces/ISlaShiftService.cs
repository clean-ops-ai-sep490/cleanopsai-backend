using CleanOpsAi.Modules.ClientManagement.Application.Dtos;
using CleanOpsAi.Modules.ClientManagement.Application.Dtos.SlaShifts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.ClientManagement.Application.Interfaces
{
    public interface ISlaShiftService
    {
        Task<SlaShiftResponse?> GetByIdAsync(Guid id);

        Task<List<SlaShiftResponse>> GetBySlaIdAsync(Guid slaId);

        Task<PagedResponse<SlaShiftResponse>> GetAllPaginationAsync(int pageNumber, int pageSize);

        Task<SlaShiftResponse> CreateAsync(SlaShiftCreateRequest request);

        Task<SlaShiftResponse> UpdateAsync(Guid id, SlaShiftUpdateRequest request);

        Task<int> DeleteAsync(Guid id);
    }
}
