using CleanOpsAi.Modules.ClientManagement.Application.Dtos;
using CleanOpsAi.Modules.ClientManagement.Application.Dtos.Contracts;
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
    public class ContractService : IContractService
    {
        private readonly IContractRepository _repository;
        private readonly IFileStorageService _fileStorage;

        private const string CONTAINER = "contracts";

        public ContractService(
            IContractRepository repository,
            IFileStorageService fileStorage)
        {
            _repository = repository;
            _fileStorage = fileStorage;
        }

        // get by id
        public async Task<ContractResponse?> GetByIdAsync(Guid id)
        {
            var contract = await _repository.GetByIdAsync(id);

            if (contract == null)
                return null;

            return new ContractResponse
            {
                Id = contract.Id,
                Name = contract.Name,
                UrlFile = contract.UrlFile,
                ClientId = contract.ClientId,
                ClientName = contract.Client?.Name,
                Created = contract.Created,
                LastModified = contract.LastModified
            };
        }

        // get all
        public async Task<List<ContractResponse>> GetAllAsync()
        {
            var contracts = await _repository.GetAllAsync();

            return contracts.Select(c => new ContractResponse
            {
                Id = c.Id,
                Name = c.Name,
                UrlFile = c.UrlFile,
                ClientId = c.ClientId,
                ClientName = c.Client?.Name,
                Created = c.Created,
                LastModified = c.LastModified
            }).ToList();
        }

        // pagination
        public async Task<PagedResponse<ContractResponse>> GetAllPaginationAsync(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            var (items, totalCount) = await _repository.GetAllPaginationAsync(pageNumber, pageSize);

            var responses = items.Select(c => new ContractResponse
            {
                Id = c.Id,
                Name = c.Name,
                UrlFile = c.UrlFile,
                ClientId = c.ClientId,
                ClientName = c.Client?.Name,
                Created = c.Created,
                LastModified = c.LastModified
            }).ToList();

            return new PagedResponse<ContractResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalElements = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                Content = responses
            };
        }

        // create
        public async Task<ContractResponse> CreateAsync(ContractCreateRequest request)
        {
            string fileUrl = "";

            if (request.FileStream != null)
            {
                fileUrl = await _fileStorage.UploadFileAsync(
                    request.FileStream,
                    request.FileName!,
                    CONTAINER
                );
            }

            var contract = new Contract
            {
                Id = Uuid7.NewGuid(),
                Name = request.Name,
                ClientId = request.ClientId,
                UrlFile = fileUrl,
                Created = DateTime.UtcNow,
                //CreatedBy = "System",
                IsDeleted = false
            };

            await _repository.CreateAsync(contract);

            return new ContractResponse
            {
                Id = contract.Id,
                Name = contract.Name,
                UrlFile = contract.UrlFile,
                ClientId = contract.ClientId
            };
        }

        // update
        public async Task<ContractResponse?> UpdateAsync(Guid id, ContractUpdateRequest request)
        {
            var contract = await _repository.GetByIdAsync(id);

            if (contract == null)
                throw new KeyNotFoundException($"Contract with id {id} not found.");

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                contract.Name = request.Name;
            }

            if (request.FileStream != null)
            {
                var fileUrl = await _fileStorage.UploadFileAsync(
                    request.FileStream,
                    request.FileName!,
                    CONTAINER
                );

                contract.UrlFile = fileUrl;
            }

            contract.LastModified = DateTime.UtcNow;
            //contract.LastModifiedBy = "System";

            await _repository.UpdateAsync(contract);

            return new ContractResponse
            {
                Id = contract.Id,
                Name = contract.Name,
                UrlFile = contract.UrlFile,
                ClientId = contract.ClientId
            };
        }

        // delete
        public async Task<int> DeleteAsync(Guid id)
        {
            var contract = await _repository.GetByIdAsync(id);

            if (contract == null)
                throw new KeyNotFoundException($"Contract with id {id} not found.");
            return await _repository.DeleteAsync(id);
        }

        // get contracts by clientId
        public async Task<List<ContractResponse>> GetByClientIdAsync(Guid clientId)
        {
            var contracts = await _repository.GetByClientIdAsync(clientId);

            return contracts.Select(c => new ContractResponse
            {
                Id = c.Id,
                Name = c.Name,
                UrlFile = c.UrlFile,
                ClientId = c.ClientId,
                ClientName = c.Client?.Name,
                Created = c.Created,
                LastModified = c.LastModified
            }).ToList();
        }

        // get contracts by clientId with pagination
        public async Task<PagedResponse<ContractResponse>> GetByClientIdPaginationAsync(Guid clientId, int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            var (items, totalCount) = await _repository
                .GetByClientIdPaginationAsync(clientId, pageNumber, pageSize);

            var responses = items.Select(c => new ContractResponse
            {
                Id = c.Id,
                Name = c.Name,
                UrlFile = c.UrlFile,
                ClientId = c.ClientId,
                ClientName = c.Client?.Name,
                Created = c.Created,
                LastModified = c.LastModified
            }).ToList();

            return new PagedResponse<ContractResponse>
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
