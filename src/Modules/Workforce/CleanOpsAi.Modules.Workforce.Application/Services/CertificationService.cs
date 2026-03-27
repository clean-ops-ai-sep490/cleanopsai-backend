using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.Modules.Workforce.Application.Dtos;
using CleanOpsAi.Modules.Workforce.Application.Dtos.Certifications;
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
    public class CertificationService : ICertificationService
    {
        private readonly ICertificationRepository _certificationRepository;
        private readonly IUserContext _userContext;
        private readonly IDateTimeProvider _dateTimeProvider;

        public CertificationService(ICertificationRepository certificationRepository, IUserContext userContext, IDateTimeProvider dateTimeProvider)
        {
            _certificationRepository = certificationRepository;
            _userContext = userContext;
            _dateTimeProvider = dateTimeProvider;
        }

        // get by id
        public async Task<List<CertificationResponse>> GetByIdAsync(Guid id)
        {
            var certification = await _certificationRepository.GetByIdAsync(id);

            if (certification == null)
                return null;

            return new List<CertificationResponse>
            {
                new CertificationResponse
                {
                    Id = certification.Id,
                    Name = certification.Name,
                    IssuingOrganization = certification.IssuingOrganization
                }
            };
        }

        // get all
        public async Task<List<CertificationResponse>> GetAllAsync()
        {
            var certifications = await _certificationRepository.GetAllAsync();

            return certifications.Select(x => new CertificationResponse
            {
                Id = x.Id,
                Name = x.Name,
                IssuingOrganization = x.IssuingOrganization
            }).ToList();
        }

        // get all pagination
        public async Task<PagedResponse<CertificationResponse>> GetAllPaginationAsync(int pageNumber, int pageSize)
        {
            var (items, totalCount) = await _certificationRepository.GetAllPaginationAsync(pageNumber, pageSize);

            var responses = items.Select(x => new CertificationResponse
            {
                Id = x.Id,
                Name = x.Name,
                IssuingOrganization = x.IssuingOrganization
            }).ToList();

            return new PagedResponse<CertificationResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalElements = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                Content = responses
            };
        }

        // create
        public async Task<CertificationResponse> CreateAsync(CertificationCreateRequest request)
        {
            var certification = new Certification
            {
                Id = Uuid7.NewGuid(),
                Name = request.Name,
                IssuingOrganization = request.IssuingOrganization,
                Created = _dateTimeProvider.UtcNow,
                CreatedBy = _userContext.UserId.ToString(),
                IsDeleted = false
            };

            await _certificationRepository.CreateAsync(certification);

            return new CertificationResponse
            {
                Id = certification.Id,
                Name = certification.Name,
                IssuingOrganization = certification.IssuingOrganization
            };
        }

        // update
        public async Task<CertificationResponse> UpdateAsync(Guid id, CertificationUpdateRequest request)
        {
            var certification = await _certificationRepository.GetByIdAsync(id);

            if (certification == null)
                throw new KeyNotFoundException($"Certification with id {id} not found.");

            certification.Name = string.IsNullOrWhiteSpace(request.Name)
                ? certification.Name
                : request.Name;

            certification.IssuingOrganization = string.IsNullOrWhiteSpace(request.IssuingOrganization)
                ? certification.IssuingOrganization
                : request.IssuingOrganization;

            certification.LastModified = _dateTimeProvider.UtcNow;
            certification.LastModifiedBy = _userContext.UserId.ToString();

            await _certificationRepository.UpdateAsync(certification);

            return new CertificationResponse
            {
                Id = certification.Id,
                Name = certification.Name,
                IssuingOrganization = certification.IssuingOrganization
            };
        }

        // delete
        public async Task<int> DeleteAsync(Guid id)
        {
            var certification = await _certificationRepository.GetByIdAsync(id);

            if (certification == null)
                throw new KeyNotFoundException($"Certification with id {id} not found.");

            return await _certificationRepository.DeleteAsync(id);
        }
    }
}
