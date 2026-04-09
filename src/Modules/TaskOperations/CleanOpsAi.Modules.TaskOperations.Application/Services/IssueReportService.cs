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

namespace CleanOpsAi.Modules.TaskOperations.Application.Services
{
    public class IssueReportService : IIssueReportService
    {
        private readonly IIssueReportRepository _issueReportRepository;
        private readonly IMapper _mapper;
        private readonly IUserContext _userContext;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IWorkerQueryService _workerQueryService;

        public IssueReportService(
            IIssueReportRepository issueReportRepository,
            IMapper mapper,
            IUserContext userContext,
            IDateTimeProvider dateTimeProvider,
            IWorkerQueryService workerQueryService)
        {
            _issueReportRepository = issueReportRepository;
            _mapper = mapper;
            _userContext = userContext;
            _dateTimeProvider = dateTimeProvider;
            _workerQueryService = workerQueryService;
        }

        public async Task<IssueReportDto?> GetById(Guid id, CancellationToken ct = default)
        {
            var entity = await _issueReportRepository.GetByIdExistAsync(id, ct);
            if (entity == null) return null;

            var dto = _mapper.Map<IssueReportDto>(entity);
            dto.ReportedByWorkerName = await GetWorkerNameAsync(entity.ReportedByWorkerId);

            return dto;
        }

        public async Task<PaginatedResult<IssueReportDto>> Gets(PaginationRequest request, CancellationToken ct = default)
        {
            var result = await _issueReportRepository.GetsPagingAsync(request, ct);

            var dtos = _mapper.Map<List<IssueReportDto>>(result.Content);

            await EnrichWorkerNamesAsync(dtos);

            return new PaginatedResult<IssueReportDto>(
                result.PageNumber,
                result.PageSize,
                result.TotalElements,
                dtos);
        }

        public async Task<PaginatedResult<IssueReportDto>> GetsByWorkerId(Guid workerId, PaginationRequest request, CancellationToken ct = default)
        {
            var result = await _issueReportRepository.GetsByWorkerIdPagingAsync(workerId, request, ct);

            var dtos = _mapper.Map<List<IssueReportDto>>(result.Content);

            await EnrichWorkerNamesAsync(dtos);

            return new PaginatedResult<IssueReportDto>(
                result.PageNumber,
                result.PageSize,
                result.TotalElements,
                dtos);
        }

        public async Task<PaginatedResult<IssueReportDto>> GetsByTaskAssignmentId(Guid taskAssignmentId, PaginationRequest request, CancellationToken ct = default)
        {
            var result = await _issueReportRepository.GetsByTaskAssignmentIdPagingAsync(taskAssignmentId, request, ct);

            var dtos = _mapper.Map<List<IssueReportDto>>(result.Content);

            await EnrichWorkerNamesAsync(dtos);

            return new PaginatedResult<IssueReportDto>(
                result.PageNumber,
                result.PageSize,
                result.TotalElements,
                dtos);
        }

        public async Task<PaginatedResult<IssueReportDto>> GetsByStatus(IssueStatus status, PaginationRequest request, CancellationToken ct = default)
        {
            var result = await _issueReportRepository.GetsByStatusPagingAsync(status, request, ct);

            var dtos = _mapper.Map<List<IssueReportDto>>(result.Content);

            await EnrichWorkerNamesAsync(dtos);

            return new PaginatedResult<IssueReportDto>(
                result.PageNumber,
                result.PageSize,
                result.TotalElements,
                dtos);
        }

        public async Task<IssueReportDto?> Create(CreateIssueReportDto dto, CancellationToken ct = default)
        {
            var entity = _mapper.Map<IssueReport>(dto);
            entity.Status = IssueStatus.Pending; // default status when create new report
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

            var dtoResult = _mapper.Map<IssueReportDto>(entity);
            dtoResult.ReportedByWorkerName = await GetWorkerNameAsync(entity.ReportedByWorkerId);

            return dtoResult;
        }

        public async Task<IssueReportDto?> Update(Guid id, UpdateIssueReportDto dto, CancellationToken ct = default)
        {
            var entity = await _issueReportRepository.GetByIdExistAsync(id, ct);
            if (entity == null) return null;

            _mapper.Map(dto, entity);
            entity.LastModified = _dateTimeProvider.UtcNow;
            entity.LastModifiedBy = _userContext.UserId.ToString();

            await _issueReportRepository.UpdateAsync(entity, ct);

            // notifi to Target - thong bao cho manager/supervisor co issue report moi can xu ly
            // var message = new IssueReportCreatedEvent
            // {
            //     ReportId            = entity.Id,
            //     TaskAssignmentId    = entity.TaskAssignmentId,
            //     ReportedByWorkerId  = entity.ReportedByWorkerId,
            //     Description         = entity.Description,
            //     CreatedAt           = entity.Created
            // };

            var dtoResult = _mapper.Map<IssueReportDto>(entity);
            dtoResult.ReportedByWorkerName = await GetWorkerNameAsync(entity.ReportedByWorkerId);

            return dtoResult;
        }

        public async Task<IssueReportDto?> Resolve(Guid id, ResolveIssueReportDto dto, CancellationToken ct = default)
        {
            var entity = await _issueReportRepository.GetByIdExistAsync(id, ct);
            if (entity == null) return null;

            entity.Status = dto.Status;
            entity.ResolvedByUserId = _userContext.UserId;
            entity.ResolvedAt = dto.Status == IssueStatus.Approved
                ? _dateTimeProvider.UtcNow
                : null;

            var resolverUserName = _userContext.FullName;

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

            var dtoResult = _mapper.Map<IssueReportDto>(entity);
            dtoResult.ReportedByWorkerName = await GetWorkerNameAsync(entity.ReportedByWorkerId);
            dtoResult.ResolvedByUserName = resolverUserName;

            return dtoResult;
        }

        public async Task<bool> Delete(Guid id, CancellationToken ct = default)
        {
            var entity = await _issueReportRepository.GetByIdExistAsync(id, ct);
            if (entity == null) return false;

            await _issueReportRepository.DeleteAsync(entity, ct);
            return true;
        }

        private async Task<string?> GetWorkerNameAsync(Guid workerId)
        {
            var dict = await _workerQueryService.GetUserNames(new List<Guid> { workerId });
            return dict.GetValueOrDefault(workerId);
        }

        private async Task EnrichWorkerNamesAsync(List<IssueReportDto> dtos)
        {
            var workerIds = dtos
                .Select(x => x.ReportedByWorkerId)
                .Distinct()
                .ToList();

            if (!workerIds.Any()) return;

            var dict = await _workerQueryService.GetUserNames(workerIds);

            foreach (var dto in dtos)
            {
                dto.ReportedByWorkerName = dict.GetValueOrDefault(dto.ReportedByWorkerId);
            }
        }
    }
}