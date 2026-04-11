using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.TaskOperations.Domain.Entities;
using CleanOpsAi.Modules.TaskOperations.Domain.Enums;
using CleanOpsAi.Modules.TaskOperations.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.TaskOperations.Infrastructure.Repositories
{
    public class TaskStepExecutionImageRepository : ITaskStepExecutionImageRepository
    {
        private readonly TaskOperationsDbContext _context;

        public TaskStepExecutionImageRepository(TaskOperationsDbContext context)
        {
            _context = context;
        }

        public async Task<List<TaskStepExecutionImage>> GetActiveByExecutionIdAndTypeAsync(
            Guid executionId, CancellationToken ct = default)
        {
            return await _context.TaskStepExecutionImages
                .Where(x => x.TaskStepExecutionId == executionId)
                .ToListAsync(ct);
        }

        public async Task AddRangeAsync(
            IEnumerable<TaskStepExecutionImage> images, CancellationToken ct = default)
        {
            await _context.TaskStepExecutionImages.AddRangeAsync(images, ct);
        }

        public async Task SaveChangesAsync(CancellationToken ct = default)
        {
            await _context.SaveChangesAsync(ct);
        }

    }
}
