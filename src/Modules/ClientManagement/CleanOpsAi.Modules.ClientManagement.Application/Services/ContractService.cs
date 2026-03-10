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

        public ContractService(
            IContractRepository repository,
            IFileStorageService fileStorage)
        {
            _repository = repository;
            _fileStorage = fileStorage;
        }

        public async Task<int> CreateAsync(ContractCreateRequest request)
        {
            string fileUrl = "";

            if (request.FileStream != null)
            {
                fileUrl = await _fileStorage.UploadFileAsync(
                    request.FileStream,
                    request.FileName!,
                    "contracts"
                );
            }

            var contract = new Contract
            {
                Id = Uuid7.NewGuid(),
                Name = request.Name,
                ClientId = request.ClientId,
                UrlFile = fileUrl
            };

            return await _repository.CreateAsync(contract);
        }

        public async Task<int> UpdateAsync(Guid id, ContractUpdateRequest request)
        {
            var contract = await _repository.GetByIdAsync(id);

            if (contract == null)
                return 0;

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                contract.Name = request.Name;
            }

            if (request.FileStream != null)
            {
                var fileUrl = await _fileStorage.UploadFileAsync(
                    request.FileStream,
                    request.FileName!,
                    "contracts"
                );

                contract.UrlFile = fileUrl;
            }

            return await _repository.UpdateAsync(contract);
        }

        public async Task<int> DeleteAsync(Guid id)
        {
            return await _repository.DeleteAsync(id);
        }

        public async Task<Contract?> GetByIdAsync(Guid id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<List<Contract>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }
    }
}
