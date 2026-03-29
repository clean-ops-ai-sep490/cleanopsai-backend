using AutoMapper;
using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Response;
using CleanOpsAi.Modules.TaskOperations.Domain.Entities;
using CleanOpsAi.Modules.TaskOperations.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.TaskOperations.Application.Services
{
    public class IssueReportService : IIssueReportService
    {
        private readonly IIssueReportRepository _issueReportRepository;
        private readonly IMapper _mapper;
        private readonly IUserContext _userContext;
        private readonly IDateTimeProvider _dateTimeProvider;

        public IssueReportService(
            IIssueReportRepository issueReportRepository,
            IMapper mapper,
            IUserContext userContext,
            IDateTimeProvider dateTimeProvider)
        {
            _issueReportRepository = issueReportRepository;
            _mapper = mapper;
            _userContext = userContext;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<IssueReportDto?> GetById(Guid id, CancellationToken ct = default)
        {
            var entity = await _issueReportRepository.GetByIdExistAsync(id, ct);
            if (entity == null) return null;
            return _mapper.Map<IssueReportDto>(entity);
        }

        public async Task<PaginatedResult<IssueReportDto>> Gets(PaginationRequest request, CancellationToken ct = default)
        {
            var result = await _issueReportRepository.GetsPagingAsync(request, ct);
            return new PaginatedResult<IssueReportDto>(
                result.PageNumber,
                result.PageSize,
                result.TotalElements,
                _mapper.Map<List<IssueReportDto>>(result.Content));
        }

        public async Task<PaginatedResult<IssueReportDto>> GetsByWorkerId(Guid workerId, PaginationRequest request, CancellationToken ct = default)
        {
            var result = await _issueReportRepository.GetsByWorkerIdPagingAsync(workerId, request, ct);
            return new PaginatedResult<IssueReportDto>(
                result.PageNumber,
                result.PageSize,
                result.TotalElements,
                _mapper.Map<List<IssueReportDto>>(result.Content));
        }

        public async Task<PaginatedResult<IssueReportDto>> GetsByTaskAssignmentId(Guid taskAssignmentId, PaginationRequest request, CancellationToken ct = default)
        {
            var result = await _issueReportRepository.GetsByTaskAssignmentIdPagingAsync(taskAssignmentId, request, ct);
            return new PaginatedResult<IssueReportDto>(
                result.PageNumber,
                result.PageSize,
                result.TotalElements,
                _mapper.Map<List<IssueReportDto>>(result.Content));
        }

        public async Task<PaginatedResult<IssueReportDto>> GetsByStatus(IssueStatus status, PaginationRequest request, CancellationToken ct = default)
        {
            var result = await _issueReportRepository.GetsByStatusPagingAsync(status, request, ct);
            return new PaginatedResult<IssueReportDto>(
                result.PageNumber,
                result.PageSize,
                result.TotalElements,
                _mapper.Map<List<IssueReportDto>>(result.Content));
        }

        public async Task<IssueReportDto?> Create(CreateIssueReportDto dto, CancellationToken ct = default)
        {
            var entity = _mapper.Map<IssueReport>(dto);
            entity.Created = _dateTimeProvider.UtcNow;
            entity.CreatedBy = _userContext.UserId.ToString();

            await _issueReportRepository.AddAsync(entity, ct);

            // notifi to Target - thong bao cho manager/supervisor co issue report moi can xu ly
            // var message = new IssueReportCreatedEvent
            // {
            //     ReportId            = entity.Id,
            //     TaskAssignmentId    = entity.TaskAssignmentId,
            //     ReportedByWorkerId  = entity.ReportedByWorkerId,
            //     Description         = entity.Description,
            //     CreatedAt           = entity.Created
            // };

            return _mapper.Map<IssueReportDto>(entity);
        }

        public async Task<IssueReportDto?> Update(Guid id, UpdateIssueReportDto dto, CancellationToken ct = default)
        {
            var entity = await _issueReportRepository.GetByIdExistAsync(id, ct);
            if (entity == null) return null;

            _mapper.Map(dto, entity);
            entity.LastModified = _dateTimeProvider.UtcNow;
            entity.LastModifiedBy = _userContext.UserId.ToString();

            await _issueReportRepository.UpdateAsync(entity, ct);
            return _mapper.Map<IssueReportDto>(entity);
        }

        public async Task<IssueReportDto?> Resolve(Guid id, ResolveIssueReportDto dto, CancellationToken ct = default)
        {
            var entity = await _issueReportRepository.GetByIdExistAsync(id, ct);
            if (entity == null) return null;

            entity.Status = dto.Status;
            entity.ResolvedByUserId = _userContext.UserId;
            entity.ResolvedAt = dto.Status == IssueStatus.Approved
                ? DateTime.UtcNow
                : null;

            await _issueReportRepository.UpdateAsync(entity, ct);

            // notifi to Target - gui mail thong bao ket qua resolve cho worker
            // var message = new IssueReportResolvedEvent
            // {
            //     ReportId            = entity.Id,
            //     TaskAssignmentId    = entity.TaskAssignmentId,
            //     ReportedByWorkerId  = entity.ReportedByWorkerId,
            //     ResolvedByUserId    = entity.ResolvedByUserId,
            //     Status              = entity.Status,           // Approved | Rejected
            //     ResolvedAt          = entity.ResolvedAt
            // };
            // string routingKey = entity.Status == IssueStatus.Approved
            //     ? "issue-report.approved"
            //     : "issue-report.rejected";

            return _mapper.Map<IssueReportDto>(entity);
        }

        public async Task<bool> Delete(Guid id, CancellationToken ct = default)
        {
            var entity = await _issueReportRepository.GetByIdExistAsync(id, ct);
            if (entity == null) return false;

            await _issueReportRepository.DeleteAsync(entity, ct);
            return true;
        }
    }
}
