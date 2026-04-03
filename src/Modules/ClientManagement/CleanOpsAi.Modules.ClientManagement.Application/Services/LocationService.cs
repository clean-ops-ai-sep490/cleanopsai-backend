using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Exceptions;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.Modules.ClientManagement.Application.Dtos;
using CleanOpsAi.Modules.ClientManagement.Application.Dtos.Locations;
using CleanOpsAi.Modules.ClientManagement.Application.Interfaces;
using CleanOpsAi.Modules.ClientManagement.Domain.Entities; 

namespace CleanOpsAi.Modules.ClientManagement.Application.Services
{
    public class LocationService : ILocationService
    {
        private readonly ILocationRepository _locationRepository;
        private readonly IUserContext _userContext;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IIdGenerator _idGenerator;

		public LocationService(ILocationRepository locationRepository, IUserContext userContext, IDateTimeProvider dateTimeProvider,
			IIdGenerator idGenerator)
        {
            _locationRepository = locationRepository;
            _userContext = userContext;
            _dateTimeProvider = dateTimeProvider;
            _idGenerator = idGenerator;
		}

        // Get Location by Id
        public async Task<List<LocationResponse>> GetByIdAsync(Guid id)
        {
            var location = await _locationRepository.GetByIdAsync(id);

            if (location == null)
                return null;

            return new List<LocationResponse>
            {
                new LocationResponse
                {
                    Id = location.Id,
                    Name = location.Name,
                    Address = location.Address,
                    Street = location.Street,
                    Commune = location.Commune,
                    Province = location.Province,
                    Latitude = location.Latitude,
                    Longitude = location.Longitude,
                    ClientId = location.ClientId,
                    ClientName = location.Client?.Name!
                }
            };
        }

        // Get all Locations
        public async Task<List<LocationResponse>> GetAllAsync()
        {
            var locations = await _locationRepository.GetAllAsync();

            return locations.Select(l => new LocationResponse
            {
                Id = l.Id,
                Name = l.Name,
                Address = l.Address,
                Street = l.Street,
                Commune = l.Commune,
                Province = l.Province,
                Latitude = l.Latitude,
                Longitude = l.Longitude,
                ClientId = l.ClientId,
                ClientName = l.Client?.Name!
            }).ToList();
        }

        // Pagination
        public async Task<PagedResponse<LocationResponse>> GetAllPaginationAsync(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            var (items, totalCount) = await _locationRepository.GetAllPaginationAsync(pageNumber, pageSize);

            var responses = items.Select(l => new LocationResponse
            {
                Id = l.Id,
                Name = l.Name,
                Address = l.Address,
                Street = l.Street,
                Commune = l.Commune,
                Province = l.Province,
                Latitude = l.Latitude,
                Longitude = l.Longitude,
                ClientId = l.ClientId,
                ClientName = l.Client?.Name!
            }).ToList();

            return new PagedResponse<LocationResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalElements = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                Content = responses
            };
        }

        // Create Location
        public async Task<LocationResponse> CreateAsync(LocationCreateRequest request)
        {
            var location = new Location
            {
                Id = _idGenerator.Generate(),
                Name = request.Name,
                Address = request.Address,
                Street = request.Street,
                Commune = request.Commune,
                Province = request.Province,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                ClientId = request.ClientId,
                Created = _dateTimeProvider.UtcNow,
                CreatedBy = _userContext.UserId.ToString(),
                IsDeleted = false
            };

            await _locationRepository.CreateAsync(location);

            return new LocationResponse
            {
                Id = location.Id,
                Name = location.Name,
                Address = location.Address,
                Street = location.Street,
                Commune = location.Commune,
                Province = location.Province,
                Latitude = location.Latitude,
                Longitude = location.Longitude,
                ClientId = location.ClientId,
                ClientName = location.Client?.Name!
            };
        }

        // Update Location
        public async Task<LocationResponse> UpdateAsync(Guid id, LocationUpdateRequest request)
        {
            var location = await _locationRepository.GetByIdAsync(id);

            if (location == null)
				throw new NotFoundException(nameof(Location), id);

			location.Name = string.IsNullOrWhiteSpace(request.Name) ? location.Name : request.Name;
            location.Address = string.IsNullOrWhiteSpace(request.Address) ? location.Address : request.Address;

            location.Street = request.Street ?? location.Street;
            location.Commune = request.Commune ?? location.Commune;
            location.Province = request.Province ?? location.Province;

            location.Latitude = request.Latitude ?? location.Latitude;
            location.Longitude = request.Longitude ?? location.Longitude;

            location.LastModified = _dateTimeProvider.UtcNow;
            location.LastModifiedBy = _userContext.UserId.ToString();

            await _locationRepository.UpdateAsync(location);

            return new LocationResponse
            {
                Id = location.Id,
                Name = location.Name,
                Address = location.Address,
                Street = location.Street,
                Commune = location.Commune,
                Province = location.Province,
                Latitude = location.Latitude,
                Longitude = location.Longitude,
                ClientId = location.ClientId,
                ClientName = location.Client?.Name!
            };
        }

        // Delete
        public async Task<int> DeleteAsync(Guid id)
        {
            var location = await _locationRepository.GetByIdAsync(id);

            if (location == null)
				throw new NotFoundException(nameof(Location), id);

            return await _locationRepository.DeleteAsync(id);
        }

        // Get all locations by clientId
        public async Task<List<LocationResponse>> GetByClientIdAsync(Guid clientId)
        {
            var locations = await _locationRepository.GetByClientIdAsync(clientId);

            return locations.Select(l => new LocationResponse
            {
                Id = l.Id,
                Name = l.Name,
                Address = l.Address,
                Street = l.Street,
                Commune = l.Commune,
                Province = l.Province,
                Latitude = l.Latitude,
                Longitude = l.Longitude,
                ClientId = l.ClientId,
                ClientName = l.Client?.Name!
            }).ToList();
        }

        // Get all locations by clientId with pagination
        public async Task<PagedResponse<LocationResponse>> GetByClientIdPaginationAsync(Guid clientId, int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            var (items, totalCount) = await _locationRepository
                .GetByClientIdPaginationAsync(clientId, pageNumber, pageSize);

            var responses = items.Select(l => new LocationResponse
            {
                Id = l.Id,
                Name = l.Name,
                Address = l.Address,
                Street = l.Street,
                Commune = l.Commune,
                Province = l.Province,
                Latitude = l.Latitude,
                Longitude = l.Longitude,
                ClientId = l.ClientId,
                ClientName = l.Client?.Name!
            }).ToList();

            return new PagedResponse<LocationResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalElements = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                Content = responses
            };
        }

    }
}
