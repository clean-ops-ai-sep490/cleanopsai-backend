using AutoMapper;
using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Exceptions;
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
        private readonly ITaskAssignmentRepository _taskAssignmentRepository;
        private readonly IMapper _mapper;
        private readonly IUserContext _userContext;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IWorkerQueryService _workerQueryService;

        public IssueReportService(
            IIssueReportRepository issueReportRepository,
            IMapper mapper,
            IUserContext userContext,
            IDateTimeProvider dateTimeProvider,
            IWorkerQueryService workerQueryService,
            ITaskAssignmentRepository taskAssignmentRepository)
        {
            _issueReportRepository = issueReportRepository;
            _mapper = mapper;
            _userContext = userContext;
            _dateTimeProvider = dateTimeProvider;
            _workerQueryService = workerQueryService;
            _taskAssignmentRepository = taskAssignmentRepository;
        }

        // ================= GET BY ID =================
        public async Task<IssueReportDto?> GetById(Guid id, CancellationToken ct = default)
        {
            var entity = await _issueReportRepository.GetByIdExistAsync(id, ct);
            if (entity == null) return null;

            var dto = _mapper.Map<IssueReportDto>(entity);

            await EnrichSingleAsync(dto, ct);

            return dto;
        }

        // ================= GET LIST =================
        public async Task<PaginatedResult<IssueReportDto>> Gets(PaginationRequest request, CancellationToken ct = default)
        {
            var result = await _issueReportRepository.GetsPagingAsync(request, ct);

            var dtos = _mapper.Map<List<IssueReportDto>>(result.Content);

            await EnrichAsync(dtos);

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

            await EnrichAsync(dtos);

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

            await EnrichAsync(dtos);

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

            await EnrichAsync(dtos);

            return new PaginatedResult<IssueReportDto>(
                result.PageNumber,
                result.PageSize,
                result.TotalElements,
                dtos);
        }

        // ================= CREATE =================
        public async Task<IssueReportDto?> Create(CreateIssueReportDto dto, CancellationToken ct = default)
        {
            var task = await _taskAssignmentRepository.GetByIdAsync(dto.TaskAssignmentId, ct);

            if (task != null)
            {
                if (task.Status != TaskAssignmentStatus.Completed &&
                    task.Status != TaskAssignmentStatus.Block)
                {
                    task.Status = TaskAssignmentStatus.Block;
                    task.LastModified = _dateTimeProvider.UtcNow;
                    task.LastModifiedBy = _userContext.UserId.ToString();

                    await _taskAssignmentRepository.SaveChangesAsync(ct);
                }
                else{
                    throw new BadRequestException("Cannot report issue for a completed or already blocked task.");
                }
            }
            var entity = _mapper.Map<IssueReport>(dto);

            entity.Status = IssueStatus.Pending;
            entity.Created = _dateTimeProvider.UtcNow;
            entity.CreatedBy = _userContext.UserId.ToString();

            await _issueReportRepository.AddAsync(entity, ct);

            var result = _mapper.Map<IssueReportDto>(entity);

            await EnrichSingleAsync(result, ct);

            return result;
        }

        // ================= UPDATE =================
        public async Task<IssueReportDto?> Update(Guid id, UpdateIssueReportDto dto, CancellationToken ct = default)
        {
            var entity = await _issueReportRepository.GetByIdExistAsync(id, ct);
            if (entity == null) return null;

            _mapper.Map(dto, entity);

            entity.LastModified = _dateTimeProvider.UtcNow;
            entity.LastModifiedBy = _userContext.UserId.ToString();

            await _issueReportRepository.UpdateAsync(entity, ct);

            var result = _mapper.Map<IssueReportDto>(entity);

            await EnrichSingleAsync(result, ct);

            return result;
        }

        // ================= RESOLVE =================
        public async Task<IssueReportDto?> Resolve(Guid id, ResolveIssueReportDto dto, CancellationToken ct = default)
        {
            var entity = await _issueReportRepository.GetByIdExistAsync(id, ct);
            if (entity == null) return null;

            entity.Status = dto.Status;
            entity.ResolvedByUserId = _userContext.UserId;
            entity.ResolvedAt = dto.Status == IssueStatus.Approved
                ? _dateTimeProvider.UtcNow
                : null;

            var resolverName = _userContext.FullName;

            var task = await _taskAssignmentRepository.GetByIdAsync(entity.TaskAssignmentId, ct);

            if (task != null)
            {
                // Nếu reject -> mở block lại
                if (dto.Status == IssueStatus.Rejected &&
                    task.Status == TaskAssignmentStatus.Block)
                {
                    task.Status = TaskAssignmentStatus.InProgress;
                    task.LastModified = _dateTimeProvider.UtcNow;
                    task.LastModifiedBy = _userContext.UserId.ToString();

                    await _taskAssignmentRepository.UpdateAsync(task.Id, task, ct);
                }
            }

            await _issueReportRepository.UpdateAsync(entity, ct);

            var result = _mapper.Map<IssueReportDto>(entity);

            result.ResolvedByUserName = resolverName;

            await EnrichSingleAsync(result, ct);

            return result;
        }

        // ================= DELETE =================
        public async Task<bool> Delete(Guid id, CancellationToken ct = default)
        {
            var entity = await _issueReportRepository.GetByIdExistAsync(id, ct);
            if (entity == null) return false;

            await _issueReportRepository.DeleteAsync(entity, ct);
            return true;
        }

        // ================= ENRICH SINGLE =================
        private async Task EnrichSingleAsync(IssueReportDto dto, CancellationToken ct)
        {
            var workerDict = await _workerQueryService.GetUserNames(
                new List<Guid> { dto.ReportedByWorkerId });

            dto.ReportedByWorkerName =
                workerDict.GetValueOrDefault(dto.ReportedByWorkerId);

            var task = await _taskAssignmentRepository.GetByIdAsync(dto.TaskAssignmentId, ct);
            dto.DisplayLocation = task?.DisplayLocation;
        }

        // ================= ENRICH LIST =================
        private async Task EnrichAsync(List<IssueReportDto> dtos)
        {
            if (!dtos.Any()) return;

            var workerIds = dtos.Select(x => x.ReportedByWorkerId).Distinct().ToList();
            var taskIds = dtos.Select(x => x.TaskAssignmentId).Distinct().ToList();

            var workerTask = _workerQueryService.GetUserNames(workerIds);
            var taskTask = _taskAssignmentRepository.GetByIdsAsync(taskIds, CancellationToken.None);

            await Task.WhenAll(workerTask, taskTask);

            var workerDict = workerTask.Result;
            var tasks = taskTask.Result;

            var taskDict = tasks.ToDictionary(x => x.Id, x => x.DisplayLocation);

            foreach (var dto in dtos)
            {
                dto.ReportedByWorkerName =
                    workerDict.GetValueOrDefault(dto.ReportedByWorkerId);

                dto.DisplayLocation =
                    taskDict.GetValueOrDefault(dto.TaskAssignmentId);
            }
        }
    }
}