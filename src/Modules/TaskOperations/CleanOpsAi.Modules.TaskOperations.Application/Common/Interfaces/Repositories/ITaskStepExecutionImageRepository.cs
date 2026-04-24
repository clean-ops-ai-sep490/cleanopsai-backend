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
            Guid executionId, ImageType imageType, CancellationToken ct = default);

        /// <summary>
        /// Returns all non-deleted images for the given execution, regardless of image type.
        /// Used by the scoring consumer to match results by URL.
        /// </summary>
        Task<List<TaskStepExecutionImage>> GetByExecutionIdAsync(
            Guid executionId, CancellationToken ct = default);

        Task AddRangeAsync(IEnumerable<TaskStepExecutionImage> images, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);

        Task<List<TaskStepExecutionImage>> GetActiveByExecutionIdAsync(
            Guid taskStepExecutionId,
            CancellationToken ct = default);

    }
}
