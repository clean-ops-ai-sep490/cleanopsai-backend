using CleanOpsAi.Modules.Workforce.Application.Dtos;
using CleanOpsAi.Modules.Workforce.Application.Dtos.WorkerGps;
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
    public class WorkerGpsService : IWorkerGpsService
    {
        private readonly IWorkerGpsRepository _repository;

        public WorkerGpsService(IWorkerGpsRepository repository)
        {
            _repository = repository;
        }

        public async Task<WorkerGpsResponse?> GetByIdAsync(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);

            if (entity == null)
                return null;

            return new WorkerGpsResponse
            {
                Id = entity.Id,
                WorkerId = entity.WorkerId,
                WorkerName = entity.Worker.FullName,
                Latitude = entity.Latitude,
                Longitude = entity.Longitude,
                Created = entity.Created
            };
        }

        public async Task<List<WorkerGpsResponse>> GetAllAsync()
        {
            var items = await _repository.GetAllAsync();

            return items.Select(x => new WorkerGpsResponse
            {
                Id = x.Id,
                WorkerId = x.WorkerId,
                WorkerName = x.Worker.FullName,
                Latitude = x.Latitude,
                Longitude = x.Longitude,
                Created = x.Created
            }).ToList();
        }

        public async Task<PagedResponse<WorkerGpsResponse>> GetAllPaginationAsync(int pageNumber, int pageSize)
        {
            var (items, totalCount) = await _repository.GetAllPaginationAsync(pageNumber, pageSize);

            var responses = items.Select(x => new WorkerGpsResponse
            {
                Id = x.Id,
                WorkerId = x.WorkerId,
                WorkerName = x.Worker.FullName,
                Latitude = x.Latitude,
                Longitude = x.Longitude,
                Created = x.Created
            }).ToList();

            return new PagedResponse<WorkerGpsResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalElements = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                Content = responses
            };
        }

        public async Task<WorkerGpsResponse?> CreateAsync(WorkerGpsCreateRequest request)
        {
            var entity = new WorkerGps
            {
                Id = Uuid7.NewGuid(),
                WorkerId = request.WorkerId,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                Created = DateTime.UtcNow,
                IsDeleted = false
            };

            await _repository.CreateAsync(entity);

            return new WorkerGpsResponse
            {
                Id = entity.Id,
                WorkerId = entity.WorkerId,
                WorkerName = entity.Worker.FullName,
                Latitude = entity.Latitude,
                Longitude = entity.Longitude,
                Created = entity.Created
            };
        }

        public async Task<WorkerGpsResponse?> UpdateAsync(Guid id, WorkerGpsUpdateRequest request)
        {
            var entity = await _repository.GetByIdAsync(id);

            if (entity == null)
                return null;

            entity.Latitude = request.Latitude;
            entity.Longitude = request.Longitude;
            entity.LastModified = DateTime.UtcNow;

            await _repository.UpdateAsync(entity);

            return new WorkerGpsResponse
            {
                Id = entity.Id,
                WorkerId = entity.WorkerId,
                WorkerName = entity.Worker.FullName,
                Latitude = entity.Latitude,
                Longitude = entity.Longitude,
                Created = entity.Created
            };
        }

        public async Task<int> DeleteAsync(Guid id)
        {
            return await _repository.DeleteAsync(id);
        }
    }
}
