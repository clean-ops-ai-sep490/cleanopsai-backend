using AutoMapper;
using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Exceptions;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.BuildingBlocks.Domain.Dtos;
using CleanOpsAi.Modules.ServicePlanning.Application.Common.Interfaces.Events;
using CleanOpsAi.Modules.ServicePlanning.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.ServicePlanning.Application.Common.Interfaces.Services;
using CleanOpsAi.Modules.ServicePlanning.Application.DTOs.Request;
using CleanOpsAi.Modules.ServicePlanning.Application.DTOs.Response;
using CleanOpsAi.Modules.ServicePlanning.Application.Services;
using CleanOpsAi.Modules.ServicePlanning.Domain.Entities;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NSubstitute; 

namespace CleanOpsAi.Modules.ServicePlanning.UnitTests.Service
{
	public class TaskScheduleServiceTests
	{
		private readonly ITaskScheduleRepository _taskScheduleRepository;
		private readonly ISopStepRepository _sopStepRepository;
		private readonly ISopRepository _sopRepository;
		private readonly IMapper _mapper;
		private readonly IIdGenerator _idGenerator;
		private readonly IDateTimeProvider _dateTimeProvider;
		private readonly IUserContext _userContext;
		private readonly ITaskScheduleEventService _taskScheduleEventService;
		private readonly TaskScheduleService _service;

		public TaskScheduleServiceTests()
		{
			_taskScheduleRepository = Substitute.For<ITaskScheduleRepository>();
			_sopStepRepository = Substitute.For<ISopStepRepository>();
			_sopRepository = Substitute.For<ISopRepository>();
			_mapper = Substitute.For<IMapper>();
			_idGenerator = Substitute.For<IIdGenerator>();
			_dateTimeProvider = Substitute.For<IDateTimeProvider>();
			_userContext = Substitute.For<IUserContext>();
			_taskScheduleEventService = Substitute.For<ITaskScheduleEventService>();

			_service = new TaskScheduleService(
				_taskScheduleRepository,
				_sopStepRepository,
				_sopRepository,
				_mapper,
				_idGenerator,
				_dateTimeProvider,
				_userContext,
				_taskScheduleEventService);
		}

		[Fact]
		public async Task GetById_WhenTaskScheduleExists_ReturnsDto()
		{
			var id = Guid.NewGuid();
			var taskSchedule = new TaskSchedule { Id = id };
			var expectedDto = new TaskScheduleDto { Id = id };

			_taskScheduleRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(taskSchedule);
			_mapper.Map<TaskScheduleDto>(taskSchedule).Returns(expectedDto);

			var result = await _service.GetById(id);

			Assert.Equal(expectedDto.Id, result.Id);
		}

		[Fact]
		public async Task GetById_WhenTaskScheduleNotFound_ThrowsNotFoundException()
		{
			var id = Guid.NewGuid();
			_taskScheduleRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((TaskSchedule?)null);

			await Assert.ThrowsAsync<NotFoundException>(() => _service.GetById(id));
		}

		[Fact]
		public async Task Create_Should_ValidateAndCreateTaskSchedule()
		{
			var workAreaDetailId = Guid.NewGuid();

			var dto = new TaskScheduleCreateDto
			{
				SopId = Guid.NewGuid(),
				SlaTaskId = Guid.NewGuid(),       
				SlaShiftId = Guid.NewGuid(),
				WorkAreaId = Guid.NewGuid(),
				WorkAreaDetailId = workAreaDetailId,  
				Name = "Test Task",
				DurationMinutes = 30,
				RecurrenceType = RecurrenceType.Daily,
				RecurrenceConfig = new RecurrenceConfig { Times = new List<TimeOnly> { TimeOnly.MinValue } },
				ContractStartDate = DateOnly.FromDateTime(DateTime.UtcNow),
				ContractEndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
				IsActive = true  
			};

			var taskSchedule = new TaskSchedule
			{
				WorkAreaDetailId = workAreaDetailId,
				IsActive = true,
				DisplayLocation = "Building A - Floor 2",
				SopId = dto.SopId,
				AssigneeId = Guid.NewGuid(),
				AssigneeName = "John Doe",   
				RecurrenceConfig = "{\"times\":[\"10:00:00\"]}",    
			};

			var expectedDto = new TaskScheduleDto { Id = Guid.NewGuid() };

			_mapper.Map<TaskSchedule>(dto).Returns(taskSchedule);
			_idGenerator.Generate().Returns(expectedDto.Id);
			_userContext.UserId.Returns(Guid.NewGuid());
			_dateTimeProvider.UtcNow.Returns(DateTime.UtcNow);
			_sopStepRepository.GetListBySopId(dto.SopId).Returns(new List<SopStep>());
			_taskScheduleRepository.InsertAsync(taskSchedule, Arg.Any<CancellationToken>()).Returns(new ValueTask<EntityEntry<TaskSchedule>>());
			_taskScheduleRepository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);
			_mapper.Map<TaskScheduleDto>(taskSchedule).Returns(expectedDto);

			var result = await _service.Create(dto);

			Assert.Equal(expectedDto.Id, result.Id);
		}

		[Fact]
		public async Task Update_WhenTaskScheduleNotFound_ThrowsNotFoundException()
		{
			var id = Guid.NewGuid();
			var dto = new TaskScheduleUpdateDto { DurationMinutes = 45 };

			_taskScheduleRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((TaskSchedule?)null);

			await Assert.ThrowsAsync<NotFoundException>(() => _service.Update(id, dto));
		}

		[Fact]
		public async Task Delete_WhenTaskScheduleNotFound_ThrowsNotFoundException()
		{
			var id = Guid.NewGuid();
			_taskScheduleRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((TaskSchedule?)null);

			await Assert.ThrowsAsync<NotFoundException>(() => _service.Delete(id));
		}

		[Fact]
		public async Task Delete_WhenTaskScheduleExists_SetsIsDeletedAndReturnsTrue()
		{
			var id = Guid.NewGuid();
			var taskSchedule = new TaskSchedule { Id = id };
			_taskScheduleRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(taskSchedule);
			_taskScheduleRepository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

			var result = await _service.Delete(id);

			Assert.True(result);
			Assert.True(taskSchedule.IsDeleted);
		}

		[Fact]
		public async Task Gets_Should_ReturnPaginatedResult()
		{
			var request = new PaginationRequest { PageNumber = 1, PageSize = 10 };
			var query = new GetsTaskScheduleQuery();  

			var schedules = new List<TaskSchedule> { new TaskSchedule(), new TaskSchedule() };
			var scheduleDtos = new List<TaskScheduleDto> { new TaskScheduleDto(), new TaskScheduleDto() };

			var pagedResult = new PaginatedResult<TaskSchedule>(1, 10, 2, schedules);

			_taskScheduleRepository
				.GetsPaging(Arg.Any<GetsTaskScheduleQuery>(), request, Arg.Any<CancellationToken>())
				.Returns(pagedResult);

			_mapper
				.Map<List<TaskScheduleDto>>(Arg.Any<List<TaskSchedule>>())
				.Returns(scheduleDtos);

			var result = await _service.Gets(query, request);  

			Assert.Equal(2, result.TotalElements);
			Assert.Equal(2, result.Content.Count);
		}

		[Fact]
		public async Task Activate_WhenTaskScheduleNotFound_ThrowsNotFoundException()
		{
			var id = Guid.NewGuid();
			_taskScheduleRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((TaskSchedule?)null);

			await Assert.ThrowsAsync<NotFoundException>(() => _service.Activate(id));
		}

		[Fact]
		public async Task Deactivate_WhenTaskScheduleNotFound_ThrowsNotFoundException()
		{
			var id = Guid.NewGuid();
			_taskScheduleRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((TaskSchedule?)null);

			await Assert.ThrowsAsync<NotFoundException>(() => _service.Deactivate(id));
		}
	}
}