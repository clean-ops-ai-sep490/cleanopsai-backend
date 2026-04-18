using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Exceptions;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.Modules.ClientManagement.Application.Dtos;
using CleanOpsAi.Modules.ClientManagement.Application.Dtos.Clients;
using CleanOpsAi.Modules.ClientManagement.Application.Interfaces;
using CleanOpsAi.Modules.ClientManagement.Domain.Entities; 

namespace CleanOpsAi.Modules.ClientManagement.Application.Services
{
    public class ClientService : IClientService
    {
        private readonly IClientRepository _clientRepository;
        private readonly IUserContext _userContext;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IIdGenerator _idGenerator; 

		public ClientService(IClientRepository clientRepository, IUserContext userContext, IDateTimeProvider dateTimeProvider,
            IIdGenerator idGenerator)
        {
            _clientRepository = clientRepository;
            _userContext = userContext;
            _dateTimeProvider = dateTimeProvider;
            _idGenerator = idGenerator;
		}

        // get Client by id and return as ClientResponse
        public async Task<List<ClientResponse>> GetByIdAsync(Guid id)
        {
            var client = await _clientRepository.GetByIdAsync(id);
            if (client == null)
				throw new NotFoundException(nameof(Client), id);

			return new List<ClientResponse> { new ClientResponse { Id = client.Id, Name = client.Name, Email = client.Email } };
        }

        // get all Clients and return as List of ClientResponse
        public async Task<List<ClientResponse>> GetAllAsync()
        {
            var clients = await _clientRepository.GetAllAsync();
            return clients.Select(c => new ClientResponse { Id = c.Id, Name = c.Name, Email = c.Email }).ToList();
        }

        // get all Clients with pagination and return as List of ClientResponse
        public async Task<PagedResponse<ClientResponse>> GetAllPaginationAsync(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            var (items, totalCount) = await _clientRepository.GetAllPaginationAsync(pageNumber, pageSize);

            var responses = items.Select(c => new ClientResponse
            {
                Id = c.Id,
                Name = c.Name,
                Email = c.Email
            }).ToList();

            return new PagedResponse<ClientResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalElements = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                Content = responses
            };
        }

        // add Client and return the number of rows affected
        public async Task<ClientResponse> CreateAsync(ClientCreateRequest request)
        {
            var client = new Client
            {
                Id = _idGenerator.Generate(),
                Name = request.Name,
                Email = request.Email,
                Created = _dateTimeProvider.UtcNow,
                CreatedBy = _userContext.UserId.ToString(),
                IsDeleted = false
            };

            await _clientRepository.CreateAsync(client);

            return new ClientResponse
            {
                Id = client.Id,
                Name = client.Name,
                Email = client.Email
            };
        }

        // update Client and return the number of rows affected
        public async Task<ClientResponse> UpdateAsync(Guid id, ClientUpdateRequest request)
        {
            var client = await _clientRepository.GetByIdAsync(id);

            if (client == null)
                throw new NotFoundException(nameof(Client), id);

            client.Name = string.IsNullOrWhiteSpace(request.Name)
                ? client.Name
                : request.Name;

            client.Email = string.IsNullOrWhiteSpace(request.Email)
                ? client.Email
                : request.Email;

            client.LastModified = _dateTimeProvider.UtcNow;
            client.LastModifiedBy = _userContext.UserId.ToString();

            await _clientRepository.UpdateAsync(client);

            return new ClientResponse
            {
                Id = client.Id,
                Name = client.Name,
                Email = client.Email
            };
        } 

        public async Task<int> DeleteAsync(Guid id)
        {
            var client = await _clientRepository.GetByIdAsync(id);

            if (client == null)
				throw new NotFoundException(nameof(Client), id);

            client.IsDeleted = true;
            await _clientRepository.UpdateAsync(client);

			return await _clientRepository.UpdateAsync(client);
		}
    }
}