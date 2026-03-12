using CleanOpsAi.Modules.ClientManagement.Application.Dtos;
using CleanOpsAi.Modules.ClientManagement.Application.Dtos.SlaShifts;
using CleanOpsAi.Modules.ClientManagement.Application.Interfaces;
using CleanOpsAi.Modules.ClientManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.ClientManagement.Application.Services
{
    public class SlaShiftService : ISlaShiftService
    {
        private readonly ISlaShiftRepository _repository;
        private readonly ISlaRepository _slaRepository;

        public SlaShiftService(ISlaShiftRepository repository, ISlaRepository slaRepository)
        {
            _repository = repository;
            _slaRepository = slaRepository;
        }

        public async Task<SlaShiftResponse?> GetByIdAsync(Guid id)
        {
            var shift = await _repository.GetByIdAsync(id);

            if (shift == null)
                return null;

            return new SlaShiftResponse
            {
                Id = shift.Id,
                Name = shift.Name,
                SlaId = shift.SlaId,
                SlaName = shift.Sla?.Name,
                StartTime = shift.StartTime,
                EndTime = shift.EndTime,
                RequiredWorker = shift.RequiredWorker,
                BreakTime = shift.BreakTime
            };
        }

        public async Task<List<SlaShiftResponse>> GetBySlaIdAsync(Guid slaId)
        {
            var shifts = await _repository.GetBySlaIdAsync(slaId);

            return shifts.Select(x => new SlaShiftResponse
            {
                Id = x.Id,
                Name = x.Name,
                SlaId = x.SlaId,
                SlaName = x.Sla?.Name,
                StartTime = x.StartTime,
                EndTime = x.EndTime,
                RequiredWorker = x.RequiredWorker,
                BreakTime = x.BreakTime
            }).ToList();
        }

        public async Task<PagedResponse<SlaShiftResponse>> GetAllPaginationAsync(int pageNumber, int pageSize)
        {
            var (items, totalCount) = await _repository.GetAllPaginationAsync(pageNumber, pageSize);

            var responses = items.Select(x => new SlaShiftResponse
            {
                Id = x.Id,
                Name = x.Name,
                SlaId = x.SlaId,
                SlaName = x.Sla?.Name,
                StartTime = x.StartTime,
                EndTime = x.EndTime,
                RequiredWorker = x.RequiredWorker,
                BreakTime = x.BreakTime
            }).ToList();

            return new PagedResponse<SlaShiftResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalElements = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                Content = responses
            };
        }

        public async Task<SlaShiftResponse> CreateAsync(SlaShiftCreateRequest request)
        {
            var sla = await _slaRepository.GetByIdAsync(request.SlaId);

            if (sla == null)
                throw new Exception("SLA not found");

            var entity = new SlaShift
            {
                Id = Medo.Uuid7.NewGuid(),
                Name = request.Name,
                SlaId = request.SlaId,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                RequiredWorker = request.RequiredWorker,
                BreakTime = request.BreakTime,
                Created = DateTime.UtcNow
            };

            await _repository.CreateAsync(entity);

            return new SlaShiftResponse
            {
                Id = entity.Id,
                Name = entity.Name,
                SlaId = entity.SlaId,
                SlaName = sla.Name,
                StartTime = entity.StartTime,
                EndTime = entity.EndTime,
                RequiredWorker = entity.RequiredWorker,
                BreakTime = entity.BreakTime
            };
        }

        public async Task<SlaShiftResponse> UpdateAsync(Guid id, SlaShiftUpdateRequest request)
        {
            var shift = await _repository.GetByIdAsync(id);

            if (shift == null)
                throw new KeyNotFoundException("Shift not found");

            shift.Name = string.IsNullOrWhiteSpace(request.Name) ? shift.Name : request.Name;

            if (request.StartTime.HasValue)
                shift.StartTime = request.StartTime.Value;

            if (request.EndTime.HasValue)
                shift.EndTime = request.EndTime.Value;

            if (request.RequiredWorker.HasValue)
                shift.RequiredWorker = request.RequiredWorker.Value;

            if (request.BreakTime.HasValue)
                shift.BreakTime = request.BreakTime.Value;

            shift.LastModified = DateTime.UtcNow;

            await _repository.UpdateAsync(shift);

            return await GetByIdAsync(id);
        }

        public async Task<int> DeleteAsync(Guid id)
        {
            return await _repository.DeleteAsync(id);
        }
    }
}
