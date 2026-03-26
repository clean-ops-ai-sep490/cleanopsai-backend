using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.Modules.Workforce.Application.Dtos;
using CleanOpsAi.Modules.Workforce.Application.Dtos.Workers;
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
    public class WorkerService : IWorkerService
    {
        private readonly IWorkerRepository _workerRepository;
        private readonly IFileStorageService _fileStorage;
        private const string CONTAINER = "contracts";
        private const string AVATAR_FOLDER = "avatars";
        private readonly IUserContext _userContext;

        public WorkerService(IWorkerRepository workerRepository, IFileStorageService fileStorageService, IUserContext userContext)
        {
            _workerRepository = workerRepository;
            _fileStorage = fileStorageService;
            _userContext = userContext;
        }

        // get by id
        public async Task<List<WorkerResponse>> GetByIdAsync(Guid id)
        {
            var worker = await _workerRepository.GetByIdAsync(id);

            if (worker == null)
                return null;

            return new List<WorkerResponse>
            {
                new WorkerResponse
                {
                    Id = worker.Id,
                    UserId = worker.UserId,
                    FullName = worker.FullName,
                    DisplayAddress = worker.DisplayAddress,
                    Latitude = worker.Latitude,
                    Longitude = worker.Longitude,
                    AvatarUrl = worker.AvatarUrl,
                    TotalSkills = worker.WorkerSkills.Count,
                    TotalCertifications = worker.WorkerCertifications.Count
                }
            };
        }

        // get by user id
        public async Task<List<WorkerResponse>> GetByUserIdAsync(string userId)
        {
            var worker = await _workerRepository.GetByUserIdAsync(userId);

            if (worker == null)
                return null;

            return new List<WorkerResponse>
            {
                new WorkerResponse
                {
                    Id = worker.Id,
                    UserId = worker.UserId,
                    FullName = worker.FullName,
                    DisplayAddress = worker.DisplayAddress,
                    Latitude = worker.Latitude,
                    Longitude = worker.Longitude,
                    AvatarUrl = worker.AvatarUrl,
                    TotalSkills = worker.WorkerSkills.Count,
                    TotalCertifications = worker.WorkerCertifications.Count
                }
            };
        }

        // get all
        public async Task<List<WorkerResponse>> GetAllAsync()
        {
            var workers = await _workerRepository.GetAllAsync();

            return workers.Select(x => new WorkerResponse
            {
                Id = x.Id,
                UserId = x.UserId,
                FullName = x.FullName,
                DisplayAddress = x.DisplayAddress,
                Latitude = x.Latitude,
                Longitude = x.Longitude,
                AvatarUrl = x.AvatarUrl,
                TotalSkills = x.WorkerSkills.Count,
                TotalCertifications = x.WorkerCertifications.Count
            }).ToList();
        }

        // get all pagination
        public async Task<PagedResponse<WorkerResponse>> GetAllPaginationAsync(int pageNumber, int pageSize)
        {
            var (items, totalCount) = await _workerRepository.GetAllPaginationAsync(pageNumber, pageSize);

            var responses = items.Select(x => new WorkerResponse
            {
                Id = x.Id,
                UserId = x.UserId,
                FullName = x.FullName,
                DisplayAddress = x.DisplayAddress,
                Latitude = x.Latitude,
                Longitude = x.Longitude,
                AvatarUrl = x.AvatarUrl,
                TotalSkills = x.WorkerSkills.Count,
                TotalCertifications = x.WorkerCertifications.Count
            }).ToList();

            return new PagedResponse<WorkerResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalElements = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                Content = responses
            };
        }

        // create
        public async Task<WorkerResponse> CreateAsync(WorkerCreateRequest request)
        {
            var worker = new Worker
            {
                Id = Uuid7.NewGuid(),
                UserId = request.UserId,
                FullName = request.FullName,
                DisplayAddress = request.DisplayAddress,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                AvatarUrl = request.AvatarUrl,
                Created = DateTime.UtcNow,
                CreatedBy = _userContext.UserId.ToString(),
                IsDeleted = false
            };

            await _workerRepository.CreateAsync(worker);

            return new WorkerResponse
            {
                Id = worker.Id,
                UserId = worker.UserId,
                FullName = worker.FullName,
                DisplayAddress = worker.DisplayAddress,
                Latitude = worker.Latitude,
                Longitude = worker.Longitude,
                AvatarUrl = worker.AvatarUrl,
                TotalSkills = 0,
                TotalCertifications = 0
            };
        }

        // update
        public async Task<WorkerResponse?> UpdateAsync(Guid id, WorkerUpdateRequest request)
        {
            var worker = await _workerRepository.GetByIdAsync(id);

            if (worker == null)
                throw new KeyNotFoundException($"Worker with id {id} not found.");

            if (!string.IsNullOrWhiteSpace(request.DisplayAddress))
                worker.DisplayAddress = request.DisplayAddress;

            if (request.Latitude.HasValue)
                worker.Latitude = request.Latitude.Value;

            if (request.Longitude.HasValue)
                worker.Longitude = request.Longitude.Value;

            if(!string.IsNullOrWhiteSpace(request.FullName))
                worker.FullName = request.FullName;

            // upload avatar
            if (request.AvatarStream != null)
            {
                var fileUrl = await _fileStorage.UploadFileAsync(
                    request.AvatarStream,
                    $"{AVATAR_FOLDER}/{request.AvatarFileName}",
                    CONTAINER
                );

                worker.AvatarUrl = fileUrl;
            }

            worker.LastModified = DateTime.UtcNow;
            worker.LastModifiedBy = _userContext.UserId.ToString();

            await _workerRepository.UpdateAsync(worker);

            return new WorkerResponse
            {
                Id = worker.Id,
                UserId = worker.UserId,
                FullName = worker.FullName,
                DisplayAddress = worker.DisplayAddress,
                Latitude = worker.Latitude,
                Longitude = worker.Longitude,
                AvatarUrl = worker.AvatarUrl,
                TotalSkills = worker.WorkerSkills.Count,
                TotalCertifications = worker.WorkerCertifications.Count
            };
        }

        // delete
        public async Task<int> DeleteAsync(Guid id)
        {
            var worker = await _workerRepository.GetByIdAsync(id);

            if (worker == null)
                throw new KeyNotFoundException($"Worker with id {id} not found.");

            return await _workerRepository.DeleteAsync(id);
        }

        // get information of current worker by user id from user context
        public async Task<List<WorkerResponse>> GetInforAsync()
        {
            var worker = await _workerRepository.GetByUserIdAsync(_userContext.UserId.ToString());

            if (worker == null)
                return null;

            return new List<WorkerResponse>
            {
                new WorkerResponse
                {
                    Id = worker.Id,
                    UserId = worker.UserId,
                    FullName = worker.FullName,
                    DisplayAddress = worker.DisplayAddress,
                    Latitude = worker.Latitude,
                    Longitude = worker.Longitude,
                    AvatarUrl = worker.AvatarUrl,
                    TotalSkills = worker.WorkerSkills.Count,
                    TotalCertifications = worker.WorkerCertifications.Count
                }
            };
        }
    }
}
