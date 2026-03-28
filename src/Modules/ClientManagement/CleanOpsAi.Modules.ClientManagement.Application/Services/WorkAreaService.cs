using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.Modules.ClientManagement.Application.Dtos;
using CleanOpsAi.Modules.ClientManagement.Application.Dtos.Workareas;
using CleanOpsAi.Modules.ClientManagement.Application.Interfaces;
using CleanOpsAi.Modules.ClientManagement.Domain.Entities;
using Medo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.ClientManagement.Application.Services
{
    public class WorkAreaService : IWorkAreaService
    {
        private readonly IWorkAreaRepository _repository;
        private readonly IUserContext _userContext;
        private readonly IDateTimeProvider _dateTimeProvider;

        public WorkAreaService(IWorkAreaRepository repository, IUserContext userContext, IDateTimeProvider dateTimeProvider)
        {
            _repository = repository;
            _userContext = userContext;
            _dateTimeProvider = dateTimeProvider;
        }

        // get by id
        public async Task<WorkAreaResponse?> GetByIdAsync(Guid id)
        {
            var workArea = await _repository.GetByIdAsync(id);

            if (workArea == null)
                return null;

            return new WorkAreaResponse
            {
                Id = workArea.Id,
                Name = workArea.Name,
                ZoneId = workArea.ZoneId,
                ZoneName = workArea.Zone?.Name,
                Created = workArea.Created,
                LastModified = workArea.LastModified
            };
        }

        // get all
        public async Task<List<WorkAreaResponse>> GetAllAsync()
        {
            var items = await _repository.GetAllAsync();

            return items.Select(w => new WorkAreaResponse
            {
                Id = w.Id,
                Name = w.Name,
                ZoneId = w.ZoneId,
                ZoneName = w.Zone?.Name,
                Created = w.Created,
                LastModified = w.LastModified
            }).ToList();
        }

        // get all with pagination
        public async Task<PagedResponse<WorkAreaResponse>> GetAllPaginationAsync(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            var (items, totalCount) = await _repository.GetAllPaginationAsync(pageNumber, pageSize);

            var responses = items.Select(w => new WorkAreaResponse
            {
                Id = w.Id,
                Name = w.Name,
                ZoneId = w.ZoneId,
                ZoneName = w.Zone?.Name,
                Created = w.Created,
                LastModified = w.LastModified
            }).ToList();

            return new PagedResponse<WorkAreaResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalElements = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                Content = responses
            };
        }

        // get by zone id with pagination
        public async Task<PagedResponse<WorkAreaResponse>> GetByZoneIdPaginationAsync(Guid zoneId, int pageNumber, int pageSize)
        {
            var (items, totalCount) = await _repository.GetByZoneIdPaginationAsync(zoneId, pageNumber, pageSize);

            var responses = items.Select(w => new WorkAreaResponse
            {
                Id = w.Id,
                Name = w.Name,
                ZoneId = w.ZoneId,
                ZoneName = w.Zone?.Name,
                Created = w.Created,
                LastModified = w.LastModified
            }).ToList();

            return new PagedResponse<WorkAreaResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalElements = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                Content = responses
            };
        }

        // create new work area
        public async Task<WorkAreaResponse> CreateAsync(WorkAreaCreateRequest request)
        {
            var workArea = new WorkArea
            {
                Id = Uuid7.NewGuid(),
                Name = request.Name,
                ZoneId = request.ZoneId,
                Created = _dateTimeProvider.UtcNow,
                CreatedBy = _userContext.UserId.ToString(),
                IsDeleted = false
            };

            await _repository.CreateAsync(workArea);

            return new WorkAreaResponse
            {
                Id = workArea.Id,
                Name = workArea.Name,
                ZoneId = workArea.ZoneId
            };
        }

        // update existing work area
        public async Task<WorkAreaResponse?> UpdateAsync(Guid id, WorkAreaUpdateRequest request)
        {
            var workArea = await _repository.GetByIdAsync(id);

            if (workArea == null)
                throw new KeyNotFoundException($"WorkArea with id {id} not found.");

            workArea.Name = string.IsNullOrWhiteSpace(request.Name) ? workArea.Name : request.Name;

            workArea.LastModified = _dateTimeProvider.UtcNow;
            workArea.LastModifiedBy = _userContext.UserId.ToString();

            await _repository.UpdateAsync(workArea);

            return new WorkAreaResponse
            {
                Id = workArea.Id,
                Name = workArea.Name,
                ZoneId = workArea.ZoneId
            };
        }

        //  delete work area by id (soft delete)
        public async Task<int> DeleteAsync(Guid id)
        {
            var workArea = await _repository.GetByIdAsync(id);

            if (workArea == null)
                throw new KeyNotFoundException($"WorkArea with id {id} not found.");
            return await _repository.DeleteAsync(id);
        }
    }
}
