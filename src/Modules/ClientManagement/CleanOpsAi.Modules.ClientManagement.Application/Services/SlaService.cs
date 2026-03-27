using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.Modules.ClientManagement.Application.Dtos;
using CleanOpsAi.Modules.ClientManagement.Application.Dtos.Slas;
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
    public class SlaService : ISlaService
    {
        private readonly ISlaRepository _slaRepository;
        private readonly IWorkAreaRepository _workAreaRepository;
        private readonly IContractRepository _contractRepository;
        private readonly IUserContext _userContext;
        private readonly IDateTimeProvider _dateTimeProvider;

        public SlaService(ISlaRepository slaRepository, IContractRepository contractRepository, IWorkAreaRepository workAreaRepository, IUserContext userContext, IDateTimeProvider dateTimeProvider)
        {
            _slaRepository = slaRepository;
            _contractRepository = contractRepository;
            _workAreaRepository = workAreaRepository;
            _userContext = userContext;
            _dateTimeProvider = dateTimeProvider;
        }

        // get by id
        public async Task<List<SlaResponse>> GetByIdAsync(Guid id)
        {
            var sla = await _slaRepository.GetByIdAsync(id);

            if (sla == null)
                return null;

            return new List<SlaResponse>
            {
                new SlaResponse
                {
                    Id = sla.Id,
                    Name = sla.Name,
                    Description = sla.Description,
                    EnvironmentTypeId = sla.EnvironmentTypeId,
                    ServiceType = sla.ServiceType,
                    WorkAreaId = sla.WorkAreaId,
                    WorkAreaName = sla.WorkArea?.Name, // Assuming WorkArea has a Name property
                    ContractId = sla.ContractId,
                    ContractName = sla.Contract?.Name // Assuming Contract has a Name property  
                }
            };
        }

        // get all
        public async Task<List<SlaResponse>> GetAllAsync()
        {
            var slas = await _slaRepository.GetAllAsync();

            return slas.Select(x => new SlaResponse
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                EnvironmentTypeId = x.EnvironmentTypeId,
                ServiceType = x.ServiceType,
                WorkAreaId = x.WorkAreaId,
                WorkAreaName = x.WorkArea?.Name, // Assuming WorkArea has a Name property
                ContractId = x.ContractId,
                ContractName = x.Contract?.Name // Assuming Contract has a Name property
            }).ToList();
        }

        // get all with pagination
        public async Task<PagedResponse<SlaResponse>> GetAllPaginationAsync(int pageNumber, int pageSize)
        {
            var (items, totalCount) = await _slaRepository.GetAllPaginationAsync(pageNumber, pageSize);

            var responses = items.Select(x => new SlaResponse
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                EnvironmentTypeId = x.EnvironmentTypeId,
                ServiceType = x.ServiceType,
                WorkAreaId = x.WorkAreaId,
                WorkAreaName = x.WorkArea?.Name, // Assuming WorkArea has a Name property
                ContractId = x.ContractId,
                ContractName = x.Contract?.Name // Assuming Contract has a Name property
            }).ToList();

            return new PagedResponse<SlaResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalElements = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                Content = responses
            };
        }

        // filter with pagination
        public async Task<PagedResponse<SlaResponse>> FilterAsync(
            Guid? workAreaId,
            Guid? contractId,
            int pageNumber,
            int pageSize)
        {
            var (items, totalCount) =
                await _slaRepository.FilterPaginationAsync(workAreaId, contractId, pageNumber, pageSize);

            var responses = items.Select(x => new SlaResponse
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                EnvironmentTypeId = x.EnvironmentTypeId,
                ServiceType = x.ServiceType,
                WorkAreaId = x.WorkAreaId,
                WorkAreaName = x.WorkArea?.Name, // Assuming WorkArea has a Name property
                ContractId = x.ContractId,
                ContractName = x.Contract?.Name // Assuming Contract has a Name property
            }).ToList();

            return new PagedResponse<SlaResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalElements = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                Content = responses
            };
        }

        // create
        public async Task<SlaResponse> CreateAsync(SlaCreateRequest request)
        {
            var workArea = await _workAreaRepository.GetByIdAsync(request.WorkAreaId);
            if (workArea == null)
                throw new Exception("WorkArea not found");

            var contract = await _contractRepository.GetByIdAsync(request.ContractId);
            if (contract == null)
                throw new Exception("Contract not found");

            var sla = new Sla
            {
                Id = Uuid7.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                EnvironmentTypeId = request.EnvironmentTypeId,
                ServiceType = request.ServiceType,
                WorkAreaId = request.WorkAreaId,
                ContractId = request.ContractId,
                Created = _dateTimeProvider.UtcNow,
                CreatedBy = _userContext.UserId.ToString(),
                IsDeleted = false
            };

            await _slaRepository.CreateAsync(sla);

            return new SlaResponse
            {
                Id = sla.Id,
                Name = sla.Name,
                Description = sla.Description,
                EnvironmentTypeId = sla.EnvironmentTypeId,
                ServiceType = sla.ServiceType,
                WorkAreaId = sla.WorkAreaId,
                WorkAreaName = sla.WorkArea.Name,
                ContractId = sla.ContractId,
                ContractName = sla.Contract.Name,
            };
        }

        // update
        public async Task<SlaResponse> UpdateAsync(Guid id, SlaUpdateRequest request)
        {
            var sla = await _slaRepository.GetByIdAsync(id);

            if (sla == null)
                throw new KeyNotFoundException($"Sla with id {id} not found.");

            sla.Name = string.IsNullOrWhiteSpace(request.Name) ? sla.Name : request.Name;
            sla.Description = string.IsNullOrWhiteSpace(request.Description) ? sla.Description : request.Description;

            if (request.EnvironmentTypeId.HasValue)
                sla.EnvironmentTypeId = request.EnvironmentTypeId.Value;

            if (request.ServiceType.HasValue)
                sla.ServiceType = request.ServiceType.Value;

            sla.LastModified = _dateTimeProvider.UtcNow;
            sla.LastModifiedBy = _userContext.UserId.ToString();

            await _slaRepository.UpdateAsync(sla);

            return new SlaResponse
            {
                Id = sla.Id,
                Name = sla.Name,
                Description = sla.Description,
                EnvironmentTypeId = sla.EnvironmentTypeId,
                ServiceType = sla.ServiceType,
                WorkAreaId = sla.WorkAreaId,
                WorkAreaName = sla.WorkArea.Name,
                ContractId = sla.ContractId,
                ContractName = sla.Contract.Name,
            };
        }

        // delete (soft delete)
        public async Task<int> DeleteAsync(Guid id)
        {
            var sla = await _slaRepository.GetByIdAsync(id);

            if (sla == null)
                throw new KeyNotFoundException($"Sla with id {id} not found.");

            return await _slaRepository.DeleteAsync(id);
        }
    }
}