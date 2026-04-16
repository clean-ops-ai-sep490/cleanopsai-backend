using AutoMapper;
using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Common;
using CleanOpsAi.BuildingBlocks.Application.Exceptions;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.BuildingBlocks.Domain.Dtos;
using CleanOpsAi.BuildingBlocks.Domain.Dtos.Sops;
using CleanOpsAi.BuildingBlocks.Infrastructure.Events;
using CleanOpsAi.Modules.ServicePlanning.Application.Common.Interfaces.Events;
using CleanOpsAi.Modules.ServicePlanning.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.ServicePlanning.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.ServicePlanning.Application.DTOs;
using CleanOpsAi.Modules.ServicePlanning.Domain.Entities;
using System.Text.Json;

namespace CleanOpsAi.Modules.ServicePlanning.Application.Services
{
	public class TaskScheduleService : ITaskScheduleService
	{
		private readonly ITaskScheduleRepository _taskScheduleRepository;
		private readonly ISopStepRepository _sopStepRepository;
		private readonly ISopRepository _sopRepository;
		private readonly IMapper _mapper;
		private readonly IIdGenerator _idGenerator;
		private readonly IDateTimeProvider _dateTimeProvider;
		private readonly IUserContext _userContext;
		private readonly ITaskScheduleEventService _taskScheduleEventService;

		public TaskScheduleService(
			ITaskScheduleRepository taskScheduleRepository,
			ISopStepRepository sopStepRepository,
			ISopRepository sopRepository,
			IMapper mapper,
			IIdGenerator idGenerator,
			IDateTimeProvider dateTimeProvider,
			IUserContext userContext,
			ITaskScheduleEventService taskScheduleEventService)
		{
			_taskScheduleRepository = taskScheduleRepository;
			_sopRepository = sopRepository;
			_sopStepRepository = sopStepRepository;
			_mapper = mapper;
			_idGenerator = idGenerator;
			_dateTimeProvider = dateTimeProvider;
			_userContext = userContext;
			_taskScheduleEventService = taskScheduleEventService;
		}

		public async Task<TaskScheduleDto> GetById(Guid id, CancellationToken ct = default)
		{
			var taskSchedule = await _taskScheduleRepository.GetByIdAsync(id, ct);
			if (taskSchedule == null)
				throw new NotFoundException(nameof(TaskSchedule), id);

			return _mapper.Map<TaskScheduleDto>(taskSchedule);
		}

		public async Task<TaskScheduleDto> Create(TaskScheduleCreateDto dto, CancellationToken ct = default)
		{
			if (dto.DurationMinutes <= 0)
				throw new BadRequestException("Duration must be greater than 0");

			if (dto.RecurrenceConfig?.Times != null)
			{
				ValidateTimesNotOverlap(dto.RecurrenceConfig.Times, dto.DurationMinutes);
			}

			ValidateContractDate(dto.ContractStartDate, dto.ContractEndDate);

			var windowStart = DateOnly.FromDateTime(_dateTimeProvider.UtcNow);
			var windowEnd = windowStart.AddDays(14);

			var hasConflict = await HasScheduleConflictAsync(
				dto.WorkAreaDetailId,
				dto.SlaShiftId,
				dto.AssigneeId,
				dto.RecurrenceType,
				dto.RecurrenceConfig!,
				dto.ContractStartDate,
				dto.ContractEndDate,
				windowStart,
				windowEnd
			);

			if (hasConflict)
				throw new BadRequestException("Schedule conflict detected");

			var taskSchedule = _mapper.Map<TaskSchedule>(dto);
			if (taskSchedule.IsActive)
			{
				ValidateForActivation(taskSchedule);
			}

			taskSchedule.Id = _idGenerator.Generate();
			taskSchedule.Created = _dateTimeProvider.UtcNow;
			taskSchedule.CreatedBy = _userContext.UserId.ToString();
			taskSchedule.Version = 1;

			var sopSteps = await _sopStepRepository.GetListBySopId(taskSchedule.SopId);
			taskSchedule.Metadata = JsonSerializer.Serialize(sopSteps);

			await _taskScheduleRepository.InsertAsync(taskSchedule);
			await _taskScheduleRepository.SaveChangesAsync();
			return _mapper.Map<TaskScheduleDto>(taskSchedule);
		}

		public async Task<TaskScheduleDto> Update(Guid id, TaskScheduleUpdateDto dto, CancellationToken ct = default)
		{
			var taskSchedule = await _taskScheduleRepository.GetByIdAsync(id, ct);
			if (taskSchedule == null)
				throw new NotFoundException(nameof(TaskSchedule), id);

			if (dto.DurationMinutes <= 0)
				throw new BadRequestException("Duration must be greater than 0");

			if (dto.RecurrenceConfig == null)
				throw new BadRequestException("RecurrenceConfig is required");

			var recurrenceConfig = dto.RecurrenceConfig;
			if (recurrenceConfig.Times != null)
			{
				ValidateTimesNotOverlap(recurrenceConfig.Times, dto.DurationMinutes);
			}

			var startDate = dto.ContractStartDate != default ? dto.ContractStartDate : taskSchedule.ContractStartDate; 

			var endDate = dto.ContractEndDate ?? taskSchedule.ContractEndDate; 
			ValidateContractDate(startDate, endDate);

			var recurrenceType = dto.RecurrenceType != default
			? dto.RecurrenceType
			: taskSchedule.RecurrenceType;


			var windowStart = DateOnly.FromDateTime(_dateTimeProvider.UtcNow);
			var windowEnd = windowStart.AddDays(14);

			var recurrenceConfigObj = JsonSerializer.Deserialize<RecurrenceConfig>(
				taskSchedule.RecurrenceConfig
			)!;

			var hasConflict = await HasScheduleConflictAsync(
				dto.WorkAreaDetailId ?? taskSchedule.WorkAreaDetailId,
				dto.SlaShiftId != Guid.Empty ? dto.SlaShiftId : taskSchedule.SlaShiftId,
				dto.AssigneeId ?? taskSchedule.AssigneeId,
				recurrenceType,
				recurrenceConfig,
				startDate,
				endDate,
				windowStart,
				windowEnd,
				excludeScheduleId: taskSchedule.Id
			);

			if (hasConflict)
				throw new BadRequestException("Schedule conflict detected");

			//var isSopChanged = dto.SopId != Guid.Empty && dto.SopId != taskSchedule.SopId;

			_mapper.Map(dto, taskSchedule);

			var sopSteps = await _sopStepRepository.GetListBySopId(taskSchedule.SopId);
			taskSchedule.Metadata = JsonSerializer.Serialize(sopSteps);

			taskSchedule.Version++;
			taskSchedule.LastModified = _dateTimeProvider.UtcNow;
			taskSchedule.LastModifiedBy = _userContext.UserId.ToString(); 

			await _taskScheduleRepository.SaveChangesAsync(ct);
			return _mapper.Map<TaskScheduleDto>(taskSchedule); 

		}

		public async Task<bool> Delete(Guid id, CancellationToken ct = default)
		{
			var taskSchedule = await _taskScheduleRepository.GetByIdAsync(id, ct);
			if (taskSchedule == null)
				throw new NotFoundException(nameof(TaskSchedule), id);

			taskSchedule.IsDeleted = true;
			await _taskScheduleRepository.SaveChangesAsync(ct);
			return true;
		}

		public async Task<IReadOnlyList<ActiveTaskScheduleDto>> GetActiveSchedulesAsync()
		{
			var schedules = await _taskScheduleRepository.GetActiveSchedulesAsync();
			return _mapper.Map<IReadOnlyList<ActiveTaskScheduleDto>>(schedules);
		}

		private async Task<bool> HasScheduleConflictAsync(
			Guid? workAreaDetailId,
			Guid slaShiftId,
			Guid? assigneeId,
			RecurrenceType recurrenceType,
			RecurrenceConfig recurrenceConfig,
			DateOnly contractStartDate,
			DateOnly? contractEndDate,
			DateOnly windowStart,
			DateOnly windowEnd,
			Guid? excludeScheduleId = null,
			CancellationToken cancellationToken = default)
		{
			if (windowStart > windowEnd)
				throw new BadRequestException("windowStart must be earlier than or equal windowEnd", nameof(windowStart));

			if (contractStartDate > windowEnd || (contractEndDate.HasValue && contractEndDate < windowStart))
				return false;

			//var targetConfig = ParseRecurrenceConfig(recurrenceConfig, recurrenceType);

			var effectiveStart = contractStartDate > windowStart ? contractStartDate : windowStart;

			var effectiveEnd = (contractEndDate ?? windowEnd) < windowEnd ? contractEndDate ?? windowEnd
				: windowEnd;

			var targetOccurrences = Expand(recurrenceType, recurrenceConfig, effectiveStart, effectiveEnd);
			var occurrenceSet = new HashSet<DateTime>(targetOccurrences);

			if (occurrenceSet.Count != targetOccurrences.Count)
				return true;

			var candidates = await _taskScheduleRepository.GetConflictingCandidateSchedulesAsync(
				workAreaDetailId,
				slaShiftId,
				assigneeId,
				windowStart,
				windowEnd,
				excludeScheduleId,
				cancellationToken);

			foreach (var existing in candidates)
			{
				var existingConfig = ParseRecurrenceConfig(existing.RecurrenceConfig, existing.RecurrenceType);

				var existingStart = Max(existing.ContractStartDate, windowStart);
				var existingEnd = Min(existing.ContractEndDate ?? windowEnd, windowEnd);

				var existingOccurrences = Expand(
					existing.RecurrenceType,
					existingConfig,
					existingStart,
					existingEnd);

				foreach (var occurrence in existingOccurrences)
				{
					if (!occurrenceSet.Add(occurrence))
						return true;
				}
			}

			return false;
		}

		private static RecurrenceConfig ParseRecurrenceConfig(
		string recurrenceConfigJson,
		RecurrenceType type)
		{
			if (string.IsNullOrWhiteSpace(recurrenceConfigJson))
				throw new BadRequestException("Recurrence config may not be empty");

			var options = new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
				PropertyNameCaseInsensitive = true
			};

			var config = JsonSerializer.Deserialize<RecurrenceConfig>(recurrenceConfigJson, options)
				?? throw new BadRequestException("Failed to parse recurrence config");

			ValidateConfig(type, config);

			return config;
		}

		private static void ValidateConfig(RecurrenceType type, RecurrenceConfig config)
		{
			if (config.Times == null || !config.Times.Any())
				throw new BadRequestException("Times is required");

			switch (type)
			{
				case RecurrenceType.Weekly:
					if (config.DaysOfWeek == null || !config.DaysOfWeek.Any())
						throw new BadRequestException("DaysOfWeek is required for Weekly");
					break;

				case RecurrenceType.Monthly:
					if (config.DaysOfMonth == null || !config.DaysOfMonth.Any())
						throw new BadRequestException("DaysOfMonth is required for Monthly");
					break;

				case RecurrenceType.Yearly:
					if (config.MonthDays == null || !config.MonthDays.Any())
						throw new BadRequestException("MonthDays is required for Yearly");
					break;
			}
		}

		private IReadOnlyList<DateTime> Expand(
			RecurrenceType type, RecurrenceConfig config, DateOnly from, DateOnly to)
		{
			var times = config.Times ?? [TimeOnly.MinValue];
			var results = new List<DateTime>();

			for (var date = from; date <= to; date = date.AddDays(1))
			{
				if (!MatchesType(type, config, date)) continue;

				foreach (var time in times)
					results.Add(DateTime.SpecifyKind(date.ToDateTime(time), DateTimeKind.Utc));
			}

			return results;
		}

		private static bool MatchesType(
			RecurrenceType type, RecurrenceConfig config, DateOnly date) =>
			type switch
			{
				RecurrenceType.Daily => true,

				RecurrenceType.Weekly =>
					config.DaysOfWeek?.Contains(date.DayOfWeek) ?? false,

				RecurrenceType.Monthly =>
					config.DaysOfMonth?.Contains(date.Day) ?? false,

				RecurrenceType.Yearly =>
					config.MonthDays?.Any(m => m.Month == date.Month && m.Day == date.Day)
					?? false,

				_ => false
			};

		private static DateOnly Max(DateOnly a, DateOnly b) => a > b ? a : b;

		private static DateOnly Min(DateOnly a, DateOnly b) => a < b ? a : b;

		public async Task GenerateTaskAssigmentsAsync(GenerateTaskAssignmentsRequest request, CancellationToken ct = default)
		{
			var schedules = await _taskScheduleRepository.GetByIdsAsync(request.TaskScheduleIds, ct);

			var items = schedules.Select(schedule =>
			{
				var config = JsonSerializer.Deserialize<RecurrenceConfig>(
					schedule.RecurrenceConfig)!;

				return new GenerateTaskAssignmentItem
				{
					ScheduleId = schedule.Id,
					AssigneeId = schedule.AssigneeId,
					WorkAreaId = schedule.WorkAreaId,
					FromDate = request.FromDate,
					ToDate = request.ToDate,
					RecurrenceType = schedule.RecurrenceType,
					RecurrenceConfig = config,
					DurationMinutes = schedule.DurationMinutes,
					AssigneeName = schedule.AssigneeName!,
					DisplayLocation = schedule.DisplayLocation!,
					Source = "manual"
				};
			}).ToList();

			var chunks = items.Chunk(100).ToList();

			foreach (var chunk in chunks)
			{
				await _taskScheduleEventService.RequestGenerateAssignments(
					new GenerateTaskAssignmentsRequestedEvent
					{
						Items = chunk.ToList()
					},
					ct);
			}

			//throw new NotImplementedException();
		}

		public async Task<PaginatedResult<TaskScheduleDto>> Gets(GetsTaskScheduleQuery query, PaginationRequest request, CancellationToken ct = default)
		{
			var result = await _taskScheduleRepository.GetsPaging(query, request, ct);

			return new PaginatedResult<TaskScheduleDto>(
				result.PageNumber,
				result.PageSize,
				result.TotalElements,
				_mapper.Map<List<TaskScheduleDto>>(result.Content));
		}

		public async Task<Result> Activate(Guid id, CancellationToken ct = default)
		{
			var schedule = await _taskScheduleRepository.GetByIdAsync(id, ct);

			if (schedule == null)
				throw new NotFoundException(nameof(TaskSchedule), id);

			ValidateForActivation(schedule);

			schedule.IsActive = true;
			schedule.LastModified = _dateTimeProvider.UtcNow;
			schedule.LastModifiedBy = _userContext.UserId.ToString();
			await _taskScheduleRepository.SaveChangesAsync(ct);
			return Result.Success();
		}

		public async Task<Result> Deactivate(Guid id, CancellationToken ct = default)
		{
			var schedule = await _taskScheduleRepository.GetByIdAsync(id, ct);

			if (schedule == null)
				throw new NotFoundException(nameof(TaskSchedule), id);


			schedule.IsActive = false;
			schedule.LastModified = _dateTimeProvider.UtcNow;
			schedule.LastModifiedBy = _userContext.UserId.ToString();
			await _taskScheduleRepository.SaveChangesAsync(ct);
			return Result.Success();
		}

		private void ValidateForActivation(TaskSchedule schedule)
		{
			if (schedule.WorkAreaDetailId == null)
				throw new BadRequestException("WorkAreaDetailId is required");

			if (string.IsNullOrWhiteSpace(schedule.DisplayLocation))
				throw new BadRequestException("DisplayLocation is required");

			if (schedule.AssigneeId == null)
				throw new BadRequestException("AssigneeId is required");

			if (string.IsNullOrEmpty(schedule.AssigneeName))
			{
				throw new BadRequestException("AssigneeName is required");
			}

			if (schedule.RecurrenceConfig == null)
				throw new BadRequestException("RecurrenceConfig is required");
		}

		private void ValidateTimesNotOverlap(List<TimeOnly> times, int durationMinutes)
		{
			if (times == null || times.Count <= 1)
				return;

			var sorted = times.OrderBy(t => t).ToList();

			for (int i = 0; i < sorted.Count - 1; i++)
			{
				var currentStart = sorted[i];
				var currentEnd = currentStart.AddMinutes(durationMinutes);

				var nextStart = sorted[i + 1];

				if (nextStart < currentEnd)
				{
					throw new BadRequestException(
						$"Time overlap detected between {currentStart} and {nextStart} with duration {durationMinutes} minutes"
					);
				}
			}
		}

		private void ValidateContractDate(DateOnly start, DateOnly? end)
		{
			if (end.HasValue && end.Value <= start)
			{
				throw new BadRequestException("ContractEndDate must be greater than ContractStartDate");
			}
		}

		public async Task<List<SopStepMetadataDto>> GetSopStepsWithSchemaAsync(Guid sopId, CancellationToken ct = default)
		{
			return await _sopRepository.GetSopStepsWithSchemaAsync(sopId, ct);
		}
	}
}

