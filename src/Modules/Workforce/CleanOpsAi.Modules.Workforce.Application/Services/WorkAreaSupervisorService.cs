using CleanOpsAi.Modules.Workforce.Application.Dtos;
using CleanOpsAi.Modules.Workforce.Application.Dtos.WorkAreaSupervisors;
using CleanOpsAi.Modules.Workforce.Application.Interfaces;
using CleanOpsAi.Modules.Workforce.Domain.Entities;
using Medo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.Workforce.Application.Services
{
    public class WorkAreaSupervisorService : IWorkAreaSupervisorService
    {
        private readonly IWorkAreaSupervisorRepository _repository;

        public WorkAreaSupervisorService(IWorkAreaSupervisorRepository repository)
        {
            _repository = repository;
        }

        public async Task<WorkAreaSupervisorResponse?> GetByIdAsync(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);

            if (entity == null)
                return null;

            return new WorkAreaSupervisorResponse
            {
                Id = entity.Id,
                WorkAreaId = entity.WorkAreaId,
                WorkerId = entity.WorkerId,
                WorkerName = entity.Worker?.FullName,
                UserId = entity.UserId,
                Created = entity.Created
            };
        }

        public async Task<WorkAreaSupervisorResponse?> GetByUserIdAsync(string userId)
        {
            var entity = await _repository.GetByUserIdAsync(userId);

            if (entity == null)
                return null;

            return new WorkAreaSupervisorResponse
            {
                Id = entity.Id,
                WorkAreaId = entity.WorkAreaId,
                WorkerId = entity.WorkerId,
                WorkerName = entity.Worker?.FullName,
                UserId = entity.UserId,
                Created = entity.Created
            };
        }

        public async Task<WorkAreaSupervisorResponse?> GetByWorkerIdAsync(Guid workerId)
        {
            var entity = await _repository.GetByWorkerIdAsync(workerId);

            if (entity == null)
                return null;

            return new WorkAreaSupervisorResponse
            {
                Id = entity.Id,
                WorkAreaId = entity.WorkAreaId,
                WorkerId = entity.WorkerId,
                WorkerName = entity.Worker?.FullName,
                UserId = entity.UserId,
                Created = entity.Created
            };
        }

        public async Task<List<WorkAreaSupervisorResponse>> GetAllAsync()
        {
            var items = await _repository.GetAllAsync();

            return items.Select(entity => new WorkAreaSupervisorResponse
            {
                Id = entity.Id,
                WorkAreaId = entity.WorkAreaId,
                WorkerId = entity.WorkerId,
                WorkerName = entity.Worker?.FullName,
                UserId = entity.UserId,
                Created = entity.Created
            }).ToList();
        }

        public async Task<PagedResponse<WorkAreaSupervisorResponse>> GetAllPaginationAsync(int pageNumber, int pageSize)
        {
            var (items, totalCount) = await _repository.GetAllPaginationAsync(pageNumber, pageSize);

            var responses = items.Select(entity => new WorkAreaSupervisorResponse
            {
                Id = entity.Id,
                WorkAreaId = entity.WorkAreaId,
                WorkerId = entity.WorkerId,
                WorkerName = entity.Worker?.FullName,
                UserId = entity.UserId,
                Created = entity.Created
            }).ToList();

            return new PagedResponse<WorkAreaSupervisorResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalElements = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                Content = responses
            };
        }

        public async Task<List<WorkAreaSupervisorResponse>> GetByWorkAreaIdAsync(Guid workAreaId)
        {
            var items = await _repository.GetByWorkAreaIdAsync(workAreaId);

            return items.Select(entity => new WorkAreaSupervisorResponse
            {
                Id = entity.Id,
                WorkAreaId = entity.WorkAreaId,
                WorkerId = entity.WorkerId,
                WorkerName = entity.Worker?.FullName,
                UserId = entity.UserId,
                Created = entity.Created
            }).ToList();
        }

        public async Task<WorkAreaSupervisorResponse?> CreateAsync(WorkAreaSupervisorCreateRequest request)
        {
            // ❗ Validate
            if (request.WorkAreaId == null)
                throw new Exception("WorkAreaId không được null");

            if (string.IsNullOrEmpty(request.UserId))
                throw new Exception("UserId không được null");

            var entity = new WorkAreaSupervisor
            {
                Id = Uuid7.NewGuid(),
                WorkAreaId = request.WorkAreaId,
                WorkerId = request.WorkerId,
                UserId = request.UserId,
                Created = DateTime.UtcNow,
                IsDeleted = false
            };

            await _repository.CreateAsync(entity);

            var created = await _repository.GetByIdAsync(entity.Id);

            return new WorkAreaSupervisorResponse
            {
                Id = created!.Id,
                WorkAreaId = created.WorkAreaId,
                WorkerId = created.WorkerId,
                WorkerName = created.Worker?.FullName,
                UserId = created.UserId,
                Created = created.Created
            };
        }

        public async Task<WorkAreaSupervisorResponse?> UpdateAsync(Guid id, WorkAreaSupervisorUpdateRequest request)
        {
            var entity = await _repository.GetByIdAsync(id);

            if (entity == null)
                return null;

            entity.WorkerId = request.WorkerId ?? entity.WorkerId;
            entity.UserId = request.UserId ?? entity.UserId;
            entity.LastModified = DateTime.UtcNow;

            await _repository.UpdateAsync(entity);

            return new WorkAreaSupervisorResponse
            {
                Id = entity.Id,
                WorkAreaId = entity.WorkAreaId,
                WorkerId = entity.WorkerId,
                WorkerName = entity.Worker?.FullName,
                UserId = entity.UserId,
                Created = entity.Created
            };
        }

        public async Task<int> DeleteAsync(Guid id)
        {
            return await _repository.DeleteAsync(id);
        }

        // Gps

        public async Task<List<WorkerGpsSimpleResponse>> GetWorkersLatestGpsByWorkAreaIdAsync(Guid workAreaId)
        {
            var items = await _repository.GetWorkersLatestGpsByWorkAreaIdAsync(workAreaId);

            return items.Select(x => new WorkerGpsSimpleResponse
            {
                WorkerId = x.WorkerId,
                WorkerName = x.Worker?.FullName,
                Latitude = x.Latitude,
                Longitude = x.Longitude,
                IsConfirmed = x.IsConfirmed,
                Created = x.Created
            }).ToList();
        }
    }
}
