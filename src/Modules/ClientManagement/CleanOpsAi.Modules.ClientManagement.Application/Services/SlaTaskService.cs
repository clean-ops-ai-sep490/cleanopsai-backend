using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Exceptions;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.BuildingBlocks.Domain.Dtos;
using CleanOpsAi.Modules.ClientManagement.Application.Dtos;
using CleanOpsAi.Modules.ClientManagement.Application.Dtos.SlaTasks;
using CleanOpsAi.Modules.ClientManagement.Application.Dtos.SlaTasks.ConfigModels;
using CleanOpsAi.Modules.ClientManagement.Application.Interfaces;
using CleanOpsAi.Modules.ClientManagement.Domain.Entities; 
using System.Text.Json; 

namespace CleanOpsAi.Modules.ClientManagement.Application.Services
{
    public class SlaTaskService : ISlaTaskService
    {
        private readonly ISlaTaskRepository _repository;
        private readonly ISlaRepository _slaRepository;
        private readonly IUserContext _userContext;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IIdGenerator _idGenerator;

		private readonly string[] validRecurrenceTypes =
        {
            "Daily",
            "Weekly",
            "Monthly"
        };

        public SlaTaskService(
            ISlaTaskRepository repository, 
            ISlaRepository slaRepository, 
            IUserContext userContext, 
            IDateTimeProvider dateTimeProvider,
            IIdGenerator idGenerator
			)
        {
            _repository = repository;
            _slaRepository = slaRepository;
            _userContext = userContext;
            _dateTimeProvider = dateTimeProvider;
            _idGenerator = idGenerator;
		}

        public async Task<SlaTaskResponse> GetByIdAsync(Guid id)
        {
            var task = await _repository.GetByIdAsync(id);

            if (task == null)
				throw new NotFoundException(nameof(SlaTask), id);

			return new SlaTaskResponse
            {
                Id = task.Id,
                Name = task.Name,
                SlaId = task.SlaId,
                SlaName = task.Sla?.Name!,
                RecurrenceType = ParseType(task.RecurrenceType),
                RecurrenceConfig = DeserializeConfig(task.RecurrenceConfig)
            };
        }

        public async Task<PagedResponse<SlaTaskResponse>> GetAllPaginationAsync(int pageNumber, int pageSize)
        {
            var (items, totalCount) =
                await _repository.GetAllPaginationAsync(pageNumber, pageSize);

            var responses = items.Select(x => new SlaTaskResponse
            {
                Id = x.Id,
                Name = x.Name,
                SlaId = x.SlaId,
                SlaName = x.Sla?.Name!,
                RecurrenceType = ParseType(x.RecurrenceType),
                RecurrenceConfig = DeserializeConfig(x.RecurrenceConfig)
            }).ToList();

            return new PagedResponse<SlaTaskResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalElements = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                Content = responses
            };
        }

        public async Task<List<SlaTaskResponse>> GetBySlaIdAsync(Guid slaId)
        {
            var tasks = await _repository.GetBySlaIdAsync(slaId);

            return tasks.Select(x => new SlaTaskResponse
            {
                Id = x.Id,
                Name = x.Name,
                SlaId = x.SlaId,
                SlaName = x.Sla?.Name!,
                RecurrenceType = ParseType(x.RecurrenceType),
                RecurrenceConfig = DeserializeConfig(x.RecurrenceConfig)
            }).ToList();
        }

        public async Task<SlaTaskResponse> CreateAsync(SlaTaskCreateRequest request)
        {
			// check if RecurrenceType or RecurrenceConfig is being updated, if yes validate them
			//ValidateRecurrenceType(request.RecurrenceType);

			//ValidateRecurrenceConfig(
			//    request.RecurrenceType,
			//    request.RecurrenceConfig
			//);

			ValidateRecurrence(request.RecurrenceType, request.RecurrenceConfig);

			var sla = await _slaRepository.GetByIdAsync(request.SlaId)
		        ?? throw new NotFoundException("SLA not found");

			var task = new SlaTask
			{
				Id = _idGenerator.Generate(),
				Name = request.Name,
				SlaId = request.SlaId, 
				RecurrenceType = request.RecurrenceType.ToString(),
				RecurrenceConfig = SerializeConfig(request.RecurrenceConfig), 
				Created = _dateTimeProvider.UtcNow,
				CreatedBy = _userContext.UserId.ToString(),
			};


			await _repository.CreateAsync(task);

            return new SlaTaskResponse
            {
                Id = task.Id,
                Name = task.Name,
                SlaId = task.SlaId,
                SlaName = sla.Name,
                RecurrenceType = ParseType(task.RecurrenceType),
                RecurrenceConfig = DeserializeConfig(task.RecurrenceConfig)
			};
        }

		// update
		public async Task<SlaTaskResponse> UpdateAsync(Guid id, SlaTaskUpdateRequest request)
		{
			var task = await _repository.GetByIdAsync(id)
				?? throw new KeyNotFoundException("Task not found");

			var currentType = ParseType(task.RecurrenceType);
			var currentConfig = DeserializeConfig(task.RecurrenceConfig);

			var finalType = request.RecurrenceType ?? currentType;
			var finalConfig = request.RecurrenceConfig ?? currentConfig;

			ValidateRecurrence(finalType, finalConfig);

			task.RecurrenceType = finalType.ToString();
			task.RecurrenceConfig = SerializeConfig(finalConfig);

			task.LastModified = _dateTimeProvider.UtcNow;
			task.LastModifiedBy = _userContext.UserId.ToString();

			await _repository.UpdateAsync(task);

			return new SlaTaskResponse
			{
				Id = task.Id,
				Name = task.Name,
				SlaId = task.SlaId,
				SlaName = task.Sla?.Name!,
				RecurrenceType = finalType,
				RecurrenceConfig = finalConfig
			};
		}

		public async Task<int> DeleteAsync(Guid id)
        {
            return await _repository.DeleteAsync(id);
        }

        // check RecurrenceType and RecurrenceConfig 
        private void ValidateRecurrenceType(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
                return;

            var validTypes = new[] { "Daily", "Weekly", "Monthly" };

            if (!validTypes.Contains(type))
                throw new Exception("Invalid RecurrenceType");
        }

		//private void ValidateRecurrenceConfig(string type, string config)
		//{
		//    var options = new JsonSerializerOptions
		//    {
		//        PropertyNameCaseInsensitive = true
		//    };

		//    try
		//    {
		//        switch (type)
		//        {
		//            case "Daily":

		//                var daily = JsonSerializer.Deserialize<DailyConfig>(config, options);

		//                if (daily == null ||
		//                    string.IsNullOrWhiteSpace(daily.Time) ||
		//                    daily.IntervalDays <= 0)
		//                {
		//                    throw new Exception("Invalid Daily RecurrenceConfig");
		//                }

		//                break;

		//            case "Weekly":

		//                var weekly = JsonSerializer.Deserialize<WeeklyConfig>(config, options);

		//                if (weekly == null ||
		//                    weekly.DaysOfWeek == null ||
		//                    !weekly.DaysOfWeek.Any() ||
		//                    string.IsNullOrWhiteSpace(weekly.Time))
		//                {
		//                    throw new Exception("Invalid Weekly RecurrenceConfig");
		//                }

		//                break;

		//            case "Monthly":

		//                var monthly = JsonSerializer.Deserialize<MonthlyConfig>(config, options);

		//                if (monthly == null ||
		//                    monthly.Month < 1 || monthly.Month > 12 ||
		//                    string.IsNullOrWhiteSpace(monthly.Time))
		//                {
		//                    throw new Exception("Invalid Monthly RecurrenceConfig");
		//                }

		//                int daysInMonth = DateTime.DaysInMonth(
		//                    _dateTimeProvider.UtcNow.Year,
		//                    monthly.Month
		//                );

		//                if (monthly.DayOfMonth < 1 || monthly.DayOfMonth > daysInMonth)
		//                {
		//                    throw new Exception($"Invalid day {monthly.DayOfMonth} for month {monthly.Month}");
		//                }

		//                break;
		//        }
		//    }
		//    catch (JsonException)
		//    {
		//        throw new Exception("RecurrenceConfig must be valid JSON");
		//    }
		//}

		private void ValidateRecurrence(
	RecurrenceType type,
	RecurrenceConfigSlaTask config)
		{
			if (config == null)
				throw new ArgumentException("RecurrenceConfig is required"); 

			if (config.Interval <= 0)
				throw new ArgumentException("Interval must be >= 1");

			switch (type)
			{
				case RecurrenceType.Daily:
					if (config.DaysOfWeek != null ||
						config.DaysOfMonth != null ||
						config.MonthDays != null)
						throw new ArgumentException("Daily must not contain other fields");
					break;

				case RecurrenceType.Weekly:
					if (config.DaysOfWeek == null || !config.DaysOfWeek.Any())
						throw new ArgumentException("Weekly requires DaysOfWeek");

					if (config.DaysOfMonth != null || config.MonthDays != null)
						throw new ArgumentException("Weekly must not contain other fields");
					break;

				case RecurrenceType.Monthly:
					if (config.DaysOfMonth == null || !config.DaysOfMonth.Any())
						throw new ArgumentException("Monthly requires DaysOfMonth");

					if (config.DaysOfWeek != null || config.MonthDays != null)
						throw new ArgumentException("Monthly must not contain other fields");
					break;

				case RecurrenceType.Yearly:
					if (config.MonthDays == null || !config.MonthDays.Any())
						throw new ArgumentException("Yearly requires MonthDays");

					if (config.DaysOfWeek != null || config.DaysOfMonth != null)
						throw new ArgumentException("Yearly must not contain other fields");
					break;

				default:
					throw new NotSupportedException();
			}
		}

		private RecurrenceConfigSlaTask DeserializeConfig(string json)
		{
			return JsonSerializer.Deserialize<RecurrenceConfigSlaTask>(json)
				?? throw new Exception("Invalid RecurrenceConfig JSON");
		}

		private string SerializeConfig(RecurrenceConfigSlaTask config)
		{
			return JsonSerializer.Serialize(config);
		}

		private RecurrenceType ParseType(string type)
		{
			return Enum.TryParse<RecurrenceType>(type, true, out var result)
				? result
				: throw new Exception("Invalid RecurrenceType");
		}
	}
}
