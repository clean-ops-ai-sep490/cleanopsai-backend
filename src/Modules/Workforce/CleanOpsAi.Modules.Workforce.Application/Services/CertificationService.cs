using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.Modules.Workforce.Application.Dtos;
using CleanOpsAi.Modules.Workforce.Application.Dtos.Certifications;
using CleanOpsAi.Modules.Workforce.Application.Dtos.Skills;
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
                    Category = certification.Category,
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
                Category = x.Category,
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
                Category = x.Category,
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
                Category = request.Category,
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
                Category = certification.Category,
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

            certification.Category = string.IsNullOrWhiteSpace(request.Category)
                ? certification.Category
                : request.Category;

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
                Category = certification.Category,
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

        // get all categories
        public async Task<List<string>> GetAllCategoriesAsync()
        {
            return await _certificationRepository.GetAllCategoriesAsync();
        }

        // get certifications by category
        public async Task<List<CertificationResponse>> GetByCategoryAsync(string category)
        {
            var certifications = await _certificationRepository.GetByCategoryAsync(category);

            if (string.IsNullOrWhiteSpace(category))
                return new List<CertificationResponse>();

            return certifications.Select(x => new CertificationResponse
            {
                Id = x.Id,
                Name = x.Name,
                Category = x.Category,
                IssuingOrganization = x.IssuingOrganization
            }).ToList();
        }

        public async Task<List<WorkerCertificationResponse>> GetCertificationsByWorkerIdAsync(Guid workerId)
        {
            var workerCerts = await _certificationRepository.GetCertificationsByWorkerIdAsync(workerId);

            return workerCerts.Select(x => new WorkerCertificationResponse
            {
                CertificationId = x.CertificationId,
                Name = x.Certification.Name,
                Category = x.Certification.Category,
                IssuingOrganization = x.Certification.IssuingOrganization,
                IssuedDate = x.IssuedDate,
                ExpiredAt = x.ExpiredAt
            }).ToList();
        }

    }
}
