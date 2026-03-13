namespace CleanOpsAi.Modules.ServicePlanning.Application.Common.Interfaces.Services
{
	public interface ITaskScheduleService
	{
		Task<TaskScheduleDto?> GetById(Guid id);

		Task<TaskScheduleDto> Create(TaskScheduleCreateDto createDto);

		Task<TaskScheduleDto> Update(Guid id, TaskScheduleUpdateDto dto);

		Task<bool> Delete(Guid id);
	}
}
