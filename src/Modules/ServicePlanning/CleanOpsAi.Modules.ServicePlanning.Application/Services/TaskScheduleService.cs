using AutoMapper;
using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.BuildingBlocks.Domain.Dtos;
using CleanOpsAi.BuildingBlocks.Infrastructure.Events;
using CleanOpsAi.Modules.ServicePlanning.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.ServicePlanning.Application.Common.Interfaces.Services; 
using CleanOpsAi.Modules.ServicePlanning.Application.DTOs;
using CleanOpsAi.Modules.ServicePlanning.Domain.Entities;
using MassTransit;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace CleanOpsAi.Modules.ServicePlanning.Application.Services
{
	public class TaskScheduleService : ITaskScheduleService
	{
		private readonly ITaskScheduleRepository _taskScheduleRepository;
		private readonly ISopStepRepository _sopStepRepository;
		private readonly IMapper _mapper;
		private readonly IPublishEndpoint _bus;

		public TaskScheduleService(
			ITaskScheduleRepository taskScheduleRepository,
			ISopStepRepository sopStepRepository,
			IMapper mapper, IPublishEndpoint publishEndpoint)
		{
			_taskScheduleRepository = taskScheduleRepository;
			_sopStepRepository = sopStepRepository;
			_mapper = mapper; 
			_bus = publishEndpoint;
		}

		public async Task<TaskScheduleDto?> GetById(Guid id)
		{
			var taskSchedule = await _taskScheduleRepository.GetById(id);
			return _mapper.Map<TaskScheduleDto>(taskSchedule);
		}

		public async Task<TaskScheduleDto> Create(TaskScheduleCreateDto dto)
		{
			var windowStart = DateOnly.FromDateTime(DateTime.UtcNow);
			var windowEnd = windowStart.AddDays(14); 

			var hasConflict = await HasScheduleConflictAsync(
				dto.WorkAreaDetailId,
				dto.SlaShiftId,
				dto.AssigneeId,
				dto.RecurrenceType,
				dto.RecurrenceConfig,
				dto.ContractStartDate,
				dto.ContractEndDate,
				windowStart,
				windowEnd
			);

			if (hasConflict)
				throw new ValidationException("Schedule conflict detected");

			var taskSchedule = _mapper.Map<TaskSchedule>(dto);
			taskSchedule.Id = Guid.NewGuid();
			taskSchedule.Created = DateTime.UtcNow;
			taskSchedule.CreatedBy = "admin-123";
			taskSchedule.Version = 1;

			var sopSteps = await _sopStepRepository.GetListBySopId(taskSchedule.SopId);
			taskSchedule.Metadata = JsonSerializer.Serialize(sopSteps);

			await _taskScheduleRepository.InsertAsync(taskSchedule);
			await _taskScheduleRepository.SaveChangesAsync();
			return _mapper.Map<TaskScheduleDto>(taskSchedule);
		}

		public async Task<TaskScheduleDto> Update(Guid id, TaskScheduleUpdateDto dto)
		{
			var taskSchedule = await _taskScheduleRepository.GetById(id);
			if (taskSchedule == null)
			{

				throw new KeyNotFoundException($"TaskSchedule with id {id} not found.");
			}
			else
			{
				_mapper.Map(dto, taskSchedule);

				var windowStart = DateOnly.FromDateTime(DateTime.UtcNow);
				var windowEnd = windowStart.AddDays(14);

				var recurrenceConfigObj = JsonSerializer.Deserialize<RecurrenceConfig>(
					taskSchedule.RecurrenceConfig
				)!;

				var hasConflict = await HasScheduleConflictAsync(
					taskSchedule.WorkAreaDetailId,
					taskSchedule.SlaShiftId,
					taskSchedule.AssigneeId,
					taskSchedule.RecurrenceType,
					recurrenceConfigObj,
					taskSchedule.ContractStartDate,
					taskSchedule.ContractEndDate,
					windowStart,
					windowEnd,
					excludeScheduleId: taskSchedule.Id
				);


				if (hasConflict)
					throw new ValidationException("Schedule conflict detected");

				taskSchedule.Version++;
				taskSchedule.LastModified = DateTime.UtcNow;
				taskSchedule.LastModifiedBy = "admin-123";

				if (dto.SopId != Guid.Empty && dto.SopId != taskSchedule.SopId)
				{
					var sopSteps = await _sopStepRepository.GetListBySopId(dto.SopId);
					taskSchedule.Metadata = JsonSerializer.Serialize(sopSteps);
				}

				await _taskScheduleRepository.SaveChangesAsync();
				return _mapper.Map<TaskScheduleDto>(taskSchedule);
			}

			
		}

		public async Task<bool> Delete(Guid id)
		{
			var taskSchedule = await _taskScheduleRepository.GetById(id);
			if (taskSchedule == null)
				return false;

			taskSchedule.IsDeleted = true;
			await _taskScheduleRepository.SaveChangesAsync();
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
				throw new ArgumentException("windowStart must be earlier than or equal windowEnd", nameof(windowStart));

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
				throw new ArgumentException("Recurrence config may not be empty");

			var options = new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
				PropertyNameCaseInsensitive = true
			};

			var config = JsonSerializer.Deserialize<RecurrenceConfig>(recurrenceConfigJson, options)
				?? throw new InvalidOperationException("Failed to parse recurrence config");

			ValidateConfig(type, config);

			return config;
		}

		private static void ValidateConfig(RecurrenceType type, RecurrenceConfig config)
		{
			if (config.Times == null || !config.Times.Any())
				throw new ValidationException("Times is required");

			switch (type)
			{
				case RecurrenceType.Weekly:
					if (config.DaysOfWeek == null || !config.DaysOfWeek.Any())
						throw new Exception("DaysOfWeek is required for Weekly");
					break;

				case RecurrenceType.Monthly:
					if (config.DaysOfMonth == null || !config.DaysOfMonth.Any())
						throw new Exception("DaysOfMonth is required for Monthly");
					break;

				case RecurrenceType.Yearly:
					if (config.MonthDays == null || !config.MonthDays.Any())
						throw new Exception("MonthDays is required for Yearly");
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

			foreach (var schedule in schedules)
			{
				var config = JsonSerializer.Deserialize<RecurrenceConfig>(
					schedule.RecurrenceConfig)!;

				await _bus.Publish(new GenerateTaskAssignmentsRequestedEvent(
					ScheduleId: schedule.Id,
					AssigneeId: schedule.AssigneeId,
					FromDate: request.FromDate,
					ToDate: request.ToDate,
					RecurrenceType: schedule.RecurrenceType,
					RecurrenceConfig: config,
					Source: "manual"
				), ct);
			}

			//throw new NotImplementedException();
		}

		public async Task<PaginatedResult<TaskScheduleDto>> Gets(PaginationRequest request, CancellationToken ct = default)
		{
			var result = await _taskScheduleRepository.GetsPaging(request, ct);

			return new PaginatedResult<TaskScheduleDto>(
				result.PageNumber,
				result.PageSize,
				result.TotalElements,
				_mapper.Map<List<TaskScheduleDto>>(result.Content));
		}
	}
}

