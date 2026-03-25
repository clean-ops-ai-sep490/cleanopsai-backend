using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.Modules.ClientManagement.Application.Dtos;
using CleanOpsAi.Modules.ClientManagement.Application.Dtos.Zones;
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
    public class ZoneService : IZoneService
    {
        private readonly IZoneRepository _repository;
        private readonly IUserContext _userContext;

        public ZoneService(IZoneRepository repository, IUserContext userContext)
        {
            _repository = repository;
            _userContext = userContext;
        }

        // get by id
        public async Task<ZoneResponse?> GetByIdAsync(Guid id)
        {
            var zone = await _repository.GetByIdAsync(id);

            if (zone == null)
                return null;

            return new ZoneResponse
            {
                Id = zone.Id,
                Name = zone.Name,
                Description = zone.Description,
                LocationId = zone.LocationId,
                LocationName = zone.Location?.Name,
                Created = zone.Created,
                LastModified = zone.LastModified
            };
        }

        // get all
        public async Task<List<ZoneResponse>> GetAllAsync()
        {
            var zones = await _repository.GetAllAsync();

            return zones.Select(z => new ZoneResponse
            {
                Id = z.Id,
                Name = z.Name,
                Description = z.Description,
                LocationId = z.LocationId,
                LocationName = z.Location?.Name,
                Created = z.Created,
                LastModified = z.LastModified
            }).ToList();
        }

        //  pagination
        public async Task<PagedResponse<ZoneResponse>> GetAllPaginationAsync(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            var (items, totalCount) = await _repository.GetAllPaginationAsync(pageNumber, pageSize);

            var responses = items.Select(z => new ZoneResponse
            {
                Id = z.Id,
                Name = z.Name,
                Description = z.Description,
                LocationId = z.LocationId,
                LocationName = z.Location?.Name,
                Created = z.Created,
                LastModified = z.LastModified
            }).ToList();

            return new PagedResponse<ZoneResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalElements = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                Content = responses
            };
        }

        // pagination by LocationId
        public async Task<PagedResponse<ZoneResponse>> GetByLocationIdPaginationAsync(Guid locationId, int pageNumber, int pageSize)
        {
            var (items, totalCount) = await _repository
                .GetByLocationIdPaginationAsync(locationId, pageNumber, pageSize);

            var responses = items.Select(z => new ZoneResponse
            {
                Id = z.Id,
                Name = z.Name,
                Description = z.Description,
                LocationId = z.LocationId,
                LocationName = z.Location?.Name,
                Created = z.Created,
                LastModified = z.LastModified
            }).ToList();

            return new PagedResponse<ZoneResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalElements = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                Content = responses
            };
        }

        // create new zone
        public async Task<ZoneResponse> CreateAsync(ZoneCreateRequest request)
        {
            var zone = new Zone
            {
                Id = Uuid7.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                LocationId = request.LocationId,
                Created = DateTime.UtcNow,
                CreatedBy = _userContext.UserId.ToString(),
                IsDeleted = false
            };

            await _repository.CreateAsync(zone);

            return new ZoneResponse
            {
                Id = zone.Id,
                Name = zone.Name,
                Description = zone.Description,
                LocationId = zone.LocationId
            };
        }

        // update zone
        public async Task<ZoneResponse?> UpdateAsync(Guid id, ZoneUpdateRequest request)
        {
            var zone = await _repository.GetByIdAsync(id);

            if (zone == null)
                throw new KeyNotFoundException($"Zone with id {id} not found.");

            zone.Name = string.IsNullOrWhiteSpace(request.Name) ? zone.Name : request.Name;
            zone.Description = request.Description ?? zone.Description;

            zone.LastModified = DateTime.UtcNow;
            zone.LastModifiedBy = _userContext.UserId.ToString();

            await _repository.UpdateAsync(zone);

            return new ZoneResponse
            {
                Id = zone.Id,
                Name = zone.Name,
                Description = zone.Description,
                LocationId = zone.LocationId
            };
        }

        // delete zone (soft delete)
        public async Task<int> DeleteAsync(Guid id)
        {
            var zone = await _repository.GetByIdAsync(id);

            if (zone == null)
                throw new KeyNotFoundException($"Zone with id {id} not found.");
            return await _repository.DeleteAsync(id);
        }
    }
}
