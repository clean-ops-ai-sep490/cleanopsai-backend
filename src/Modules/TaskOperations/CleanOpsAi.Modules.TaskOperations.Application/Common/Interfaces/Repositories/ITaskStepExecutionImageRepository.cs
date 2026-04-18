using CleanOpsAi.Modules.TaskOperations.Domain.Entities;
using CleanOpsAi.Modules.TaskOperations.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Repositories
{
    public interface ITaskStepExecutionImageRepository
    {
        Task<List<TaskStepExecutionImage>> GetActiveByExecutionIdAndTypeAsync(
            Guid executionId, CancellationToken ct = default);
        Task AddRangeAsync(IEnumerable<TaskStepExecutionImage> images, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);

    }
}
