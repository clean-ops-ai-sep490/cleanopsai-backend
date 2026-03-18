using CleanOpsAi.Modules.Workforce.Application.Dtos;
using CleanOpsAi.Modules.Workforce.Application.Dtos.WorkerCertifications;
using CleanOpsAi.Modules.Workforce.Application.Interfaces;
using CleanOpsAi.Modules.Workforce.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.Workforce.Application.Services
{
    public class WorkerCertificationService : IWorkerCertificationService
    {
        private readonly IWorkerCertificationRepository _repository;

        public WorkerCertificationService(IWorkerCertificationRepository repository)
        {
            _repository = repository;
        }

        // get by id
        public async Task<WorkerCertificationResponse?> GetByIdAsync(Guid workerId, Guid certificationId)
        {
            var entity = await _repository.GetByIdAsync(workerId, certificationId);

            if (entity == null)
                return null;

            return new WorkerCertificationResponse
            {
                WorkerId = entity.WorkerId,
                CertificationId = entity.CertificationId,
                WorkerName = entity.Worker.FullName,
                CertificationName = entity.Certification.Name,
                IssuedDate = entity.IssuedDate,
                ExpiredAt = entity.ExpiredAt
            };
        }

        // get all
        public async Task<List<WorkerCertificationResponse>> GetAllAsync()
        {
            var items = await _repository.GetAllAsync();

            return items.Select(x => new WorkerCertificationResponse
            {
                WorkerId = x.WorkerId,
                CertificationId = x.CertificationId,
                WorkerName = x.Worker.FullName,
                CertificationName = x.Certification.Name,
                IssuedDate = x.IssuedDate,
                ExpiredAt = x.ExpiredAt
            }).ToList();
        }

        // pagination
        public async Task<PagedResponse<WorkerCertificationResponse>> GetAllPaginationAsync(int pageNumber, int pageSize)
        {
            var (items, totalCount) = await _repository.GetAllPaginationAsync(pageNumber, pageSize);

            var responses = items.Select(x => new WorkerCertificationResponse
            {
                WorkerId = x.WorkerId,
                CertificationId = x.CertificationId,
                WorkerName = x.Worker.FullName,
                CertificationName = x.Certification.Name,
                IssuedDate = x.IssuedDate,
                ExpiredAt = x.ExpiredAt
            }).ToList();

            return new PagedResponse<WorkerCertificationResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalElements = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                Content = responses
            };
        }

        // create
        public async Task<WorkerCertificationResponse?> CreateAsync(WorkerCertificationCreateRequest request)
        {
            var entity = new WorkerCertification
            {
                WorkerId = request.WorkerId,
                CertificationId = request.CertificationId,
                IssuedDate = request.IssuedDate,
                ExpiredAt = request.ExpiredAt
            };

            await _repository.CreateAsync(entity);

            return new WorkerCertificationResponse
            {
                WorkerId = entity.WorkerId,
                CertificationId = entity.CertificationId,
                IssuedDate = entity.IssuedDate,
                ExpiredAt = entity.ExpiredAt
            };
        }

        // update
        public async Task<WorkerCertificationResponse?> UpdateAsync(Guid workerId, Guid certificationId, WorkerCertificationUpdateRequest request)
        {
            var entity = await _repository.GetByIdAsync(workerId, certificationId);

            if (entity == null)
                return null;

            entity.IssuedDate = request.IssuedDate;
            entity.ExpiredAt = request.ExpiredAt;

            await _repository.UpdateAsync(entity);

            return new WorkerCertificationResponse
            {
                WorkerId = entity.WorkerId,
                CertificationId = entity.CertificationId,
                WorkerName = entity.Worker.FullName,
                CertificationName = entity.Certification.Name,
                IssuedDate = entity.IssuedDate,
                ExpiredAt = entity.ExpiredAt
            };
        }

        // delete
        public async Task<int> DeleteAsync(Guid workerId, Guid certificationId)
        {
            var entity = await _repository.GetByIdAsync(workerId, certificationId);

            if (entity == null)
                return 0;

            return await _repository.DeleteAsync(workerId, certificationId);
        }
    }
}
