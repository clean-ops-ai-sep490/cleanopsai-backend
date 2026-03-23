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

        public WorkerGpsService(
            IWorkerGpsRepository repository,
            IWorkerRepository workerRepository)
        {
            _repository = repository;
            _workerRepository = workerRepository;
        }

        private static WorkerGpsResponse MapToResponse(WorkerGps entity) => new()
        {
            Id = entity.Id,
            WorkerId = entity.WorkerId,
            WorkerName = entity.Worker?.FullName,
            Latitude = entity.Latitude,
            Longitude = entity.Longitude,
            IsConfirmed = entity.IsConfirmed,
            Created = entity.Created
        };

        public async Task<WorkerGpsResponse?> GetByIdAsync(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<List<WorkerGpsResponse>> GetAllAsync()
        {
            var items = await _repository.GetAllAsync();
            return items.Select(MapToResponse).ToList();
        }

        public async Task<PagedResponse<WorkerGpsResponse>> GetAllPaginationAsync(
            int pageNumber, int pageSize)
        {
            var (items, totalCount) = await _repository.GetAllPaginationAsync(pageNumber, pageSize);

            return new PagedResponse<WorkerGpsResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalElements = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                Content = items.Select(MapToResponse).ToList()
            };
        }

        public async Task<WorkerGpsResponse?> CreateAsync(WorkerGpsCreateRequest request)
        {
            var worker = await _workerRepository.GetByIdAsync(request.WorkerId);

            if (worker == null)
                throw new KeyNotFoundException($"Worker with id {request.WorkerId} not found.");

            var entity = new WorkerGps
            {
                Id = Uuid7.NewGuid(),
                WorkerId = request.WorkerId,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                IsConfirmed = request.IsConfirmed,
                Created = DateTime.UtcNow,
                IsDeleted = false
            };

            await _repository.CreateAsync(entity);

            var created = await _repository.GetByIdAsync(entity.Id);
            return MapToResponse(created!);
        }

        public async Task<WorkerGpsResponse?> GetLatestByWorkerIdAsync(Guid workerId)
        {
            var entity = await _repository.GetLatestByWorkerIdAsync(workerId);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<List<WorkerGpsResponse>> GetLatestByWorkerIdsAsync(List<Guid> workerIds)
        {
            var items = await _repository.GetLatestByWorkerIdsAsync(workerIds);
            return items.Select(MapToResponse).ToList();
        }

        public async Task<PagedResponse<WorkerGpsResponse>> GetByWorkerIdPaginationAsync(
            Guid workerId, int pageNumber, int pageSize)
        {
            var (items, totalCount) = await _repository
                .GetByWorkerIdPaginationAsync(workerId, pageNumber, pageSize);

            return new PagedResponse<WorkerGpsResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalElements = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                Content = items.Select(MapToResponse).ToList()
            };
        }
    }
}
