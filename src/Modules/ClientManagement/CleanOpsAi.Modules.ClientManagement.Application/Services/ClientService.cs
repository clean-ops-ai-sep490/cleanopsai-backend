using CleanOpsAi.Modules.ClientManagement.Application.Dtos;
using CleanOpsAi.Modules.ClientManagement.Application.Dtos.Clients;
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
    public class ClientService : IClientService
    {
        private readonly IClientRepository _clientRepository;
        public ClientService(IClientRepository clientRepository)
        {
            _clientRepository = clientRepository;
        }

        // get Client by id and return as ClientResponse
        public async Task<List<ClientResponse>> GetByIdAsync(Guid id)
        {
            var client = await _clientRepository.GetByIdAsync(id);
            if (client == null)
            {
                return null;
            }
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
                Id = Uuid7.NewGuid(),
                Name = request.Name,
                Email = request.Email,
                Created = DateTime.UtcNow,
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
                throw new KeyNotFoundException($"Client with id {id} not found.");

            client.Name = string.IsNullOrWhiteSpace(request.Name)
                ? client.Name
                : request.Name;

            client.Email = string.IsNullOrWhiteSpace(request.Email)
                ? client.Email
                : request.Email;

            client.LastModified = DateTime.UtcNow;

            await _clientRepository.UpdateAsync(client);

            return new ClientResponse
            {
                Id = client.Id,
                Name = client.Name,
                Email = client.Email
            };
        }

        // delete Client and return the number of rows affected
        public async Task<int> DeleteAsync(Guid id)
        {
            var client = await _clientRepository.GetByIdAsync(id);

            if (client == null)
                throw new KeyNotFoundException($"Client with id {id} not found.");
            return await _clientRepository.DeleteAsync(id);
        }
    }
}