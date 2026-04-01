using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services
{
    public interface ISupervisorQueryService
    {
        Task<Guid?> GetSupervisorIdAsync(
            Guid workAreaId,
            Guid workerId,
            CancellationToken ct = default);
    }
}
