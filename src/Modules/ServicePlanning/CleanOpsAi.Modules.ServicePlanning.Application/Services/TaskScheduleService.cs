using AutoMapper;
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
		private readonly IMapper _mapper;

		public TaskScheduleService(ITaskScheduleRepository taskScheduleRepository,
			ISopStepRepository sopStepRepository,
			IMapper mapper)
		{
			_taskScheduleRepository = taskScheduleRepository;
			_sopStepRepository = sopStepRepository;
			_mapper = mapper;	
		} 

		public async Task<TaskScheduleDto?> GetById(Guid id)
		{
			var taskSchedule = await _taskScheduleRepository.GetActiveByIdAsync(id);
			return _mapper.Map<TaskScheduleDto>(taskSchedule);
		}

		public async Task<TaskScheduleDto> Create(TaskScheduleCreateDto dto)
		{
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
				throw new KeyNotFoundException($"TaskSchedule with id {id} not found.");

			taskSchedule.Version++;

			_mapper.Map(dto, taskSchedule);
			taskSchedule.LastModified = DateTime.UtcNow;
			taskSchedule.LastModifiedBy = "admin-123"; 

			await _taskScheduleRepository.SaveChangesAsync();
			return _mapper.Map<TaskScheduleDto>(taskSchedule);
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
	}
}
