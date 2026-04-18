using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services
{
    public interface IEquipmentQueryService
    {
        Task<Dictionary<Guid, string>> GetNamesAsync(List<Guid> ids, CancellationToken ct = default);
    }
}
