using AutoMapper;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.TaskOperations.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.TaskOperations.Application.DTOs.Response;
using CleanOpsAi.Modules.TaskOperations.Domain.Enums;

namespace CleanOpsAi.Modules.TaskOperations.Application.Services
{
	public class TaskAssignmentService : ITaskAssignmentService
	{
		private readonly ITaskAssignmentRepository _taskAssignmentRepository;
		private readonly IMapper _mapper;

		public TaskAssignmentService(ITaskAssignmentRepository taskAssignmentRepository,
			IMapper mapper)
		{
			_taskAssignmentRepository = taskAssignmentRepository;
			_mapper = mapper;
		}


		public async Task<bool> Delete(Guid id)
		{
			var taskAssignment = await _taskAssignmentRepository.GetByIdAsync(id); 
			if (taskAssignment == null) return false;

			taskAssignment.IsDeleted = true; 
			await _taskAssignmentRepository.SaveChangesAsync(); 
			return true;
		}

		public async Task<TaskAssignmentDto?> GetById(Guid id, CancellationToken ct = default)
		{
			var taskAssignment = await _taskAssignmentRepository.GetByIdExist(id, ct);
			if (taskAssignment == null) return null;

			return _mapper.Map<TaskAssignmentDto?>(taskAssignment);
		}

		public async Task<TaskAssignmentDto?> Update(Guid id, TaskAssignmentDto dto)
		{
			var taskAssignment = await _taskAssignmentRepository.GetByIdAsync(id);

			if (taskAssignment == null)
				return null;

			_mapper.Map(dto, taskAssignment);
			taskAssignment.LastModified = DateTime.UtcNow;
			taskAssignment.LastModifiedBy = "admin-123";
			 
			await _taskAssignmentRepository.SaveChangesAsync();

			return _mapper.Map<TaskAssignmentDto>(taskAssignment); 
		}

		public async Task<bool> UpdateStatus(Guid id, TaskAssignmentStatus status)
		{
			var taskAssignment = await _taskAssignmentRepository.GetByIdAsync(id);
			if (taskAssignment == null) return false;

			taskAssignment.Status = status; 
			await _taskAssignmentRepository.SaveChangesAsync(); 
			return true;

		}
	}
}
