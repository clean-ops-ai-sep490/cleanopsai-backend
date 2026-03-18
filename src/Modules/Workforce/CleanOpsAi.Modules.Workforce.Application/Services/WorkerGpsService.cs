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
        private readonly IWorkerRepository _workerRepository;

        public WorkerGpsService(IWorkerGpsRepository repository)
        {
            _repository = repository;
        }

        public async Task<WorkerGpsResponse?> GetByIdAsync(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);

            if (entity == null)
                return null;

            return MapToResponse(entity);
        }

        public async Task<List<WorkerGpsResponse>> GetAllAsync()
        {
            var items = await _repository.GetAllAsync();
            return items.Select(MapToResponse).ToList();
        }

        public async Task<PagedResponse<WorkerGpsResponse>> GetAllPaginationAsync(int pageNumber, int pageSize)
        {
            var (items, totalCount) = await _repository.GetAllPaginationAsync(pageNumber, pageSize);

            var responses = items.Select(MapToResponse).ToList();

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
            //  Validate worker tồn tại
            var worker = await _workerRepository.GetByIdAsync(request.WorkerId);

            if (worker == null)
                throw new Exception("Worker không tồn tại");

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

            //  Query lại để có navigation property
            var created = await _repository.GetByIdAsync(entity.Id);

            return MapToResponse(created!);
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

            return MapToResponse(entity);
        }

        public async Task<int> DeleteAsync(Guid id)
        {
            return await _repository.DeleteAsync(id);
        }

        //  Map riêng để tránh lặp code + tránh null crash
        private static WorkerGpsResponse MapToResponse(WorkerGps entity)
        {
            return new WorkerGpsResponse
            {
                Id = entity.Id,
                WorkerId = entity.WorkerId,
                WorkerName = entity.Worker?.FullName, //  tránh null
                Latitude = entity.Latitude,
                Longitude = entity.Longitude,
                Created = entity.Created
            };
        }
    }
}
