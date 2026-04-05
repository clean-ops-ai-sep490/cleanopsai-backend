using AutoMapper;
using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Exceptions;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.Modules.ServicePlanning.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.ServicePlanning.Application.DTOs.Request;
using CleanOpsAi.Modules.ServicePlanning.Application.DTOs.Response;
using CleanOpsAi.Modules.ServicePlanning.Application.Services;
using CleanOpsAi.Modules.ServicePlanning.Domain.Entities;
using NSubstitute;
using System.Text.Json;

namespace CleanOpsAi.Modules.ServicePlanning.UnitTests.Service
{
	public class StepServiceTests
	{
		private readonly IStepRepository _stepRepository;
		private readonly IMapper _mapper;
		private readonly IDateTimeProvider _dateTimeProvider;
		private readonly IIdGenerator _idGenerator;
		private readonly IUserContext _userContext;
		private readonly StepService _stepService;

		public StepServiceTests()
		{
			_stepRepository = Substitute.For<IStepRepository>();
			_mapper = Substitute.For<IMapper>();
			_dateTimeProvider = Substitute.For<IDateTimeProvider>();
			_idGenerator = Substitute.For<IIdGenerator>();
			_userContext = Substitute.For<IUserContext>();

			_stepService = new StepService(
				_stepRepository,
				_mapper,
				_dateTimeProvider,
				_idGenerator,
				_userContext);
		}

		[Fact]
		public async Task Gets_Should_ReturnPaginatedResult()
		{
			var request = new PaginationRequest { PageNumber = 1, PageSize = 10 };
			var steps = new List<Step> { new Step(), new Step() };
			var stepDtos = new List<StepDto> { new StepDto(), new StepDto() };

			var pagedResult = new PaginatedResult<Step>(1, 10, 2, steps);

			_stepRepository.GetsPaging(request, Arg.Any<CancellationToken>()).Returns(pagedResult);
			_mapper.Map<List<StepDto>>(Arg.Any<List<Step>>()).Returns(stepDtos);

			var result = await _stepService.Gets(request);

			Assert.Equal(2, result.TotalElements);
			Assert.Equal(2, result.Content.Count);
		}

		[Fact]
		public async Task GetStepById_WhenStepExists_ReturnsStepDto()
		{
			var id = _idGenerator.Generate();
			var step = new Step { Id = id };
			var stepDto = new StepDto { Id = id };

			_stepRepository.GetByIdAsync(id, Arg.Any<CancellationToken>())
				.Returns(step);
			_mapper.Map<StepDto>(step).Returns(stepDto);

			var result = await _stepService.GetStepById(id);

			Assert.Equal(id, result.Id);
		}

		[Fact]
		public async Task GetStepById_WhenStepNotFound_ThrowsNotFoundException()
		{
			var id = _idGenerator.Generate();
			_stepRepository.GetByIdAsync(id, Arg.Any<CancellationToken>())
				.Returns((Step?)null);

			await Assert.ThrowsAsync<NotFoundException>(
				() => _stepService.GetStepById(id));
		}

		[Fact]
		public async Task CreateNewStep_Should_SetAllAuditFields_And_ReturnMappedDto()
		{
			JsonElement configSchema = JsonDocument.Parse("""
			{
				"type": "object",
				"required": ["items"],
				"properties": {
					"items": {
						"type": "array",
						"items": {"type": "string"},
						"title": "Danh sách",
						"x-widget": "string-list",
						"x-addable": true
					}
				},
				"x-behavior": "list",
				"additionalProperties": false
			}
			""").RootElement;

			// Arrange
			var dto = new StepCreateDto
			{
				ConfigSchema = configSchema
			};

			var createdStep = new Step();
			var expectedDto = new StepDto { Id = _idGenerator.Generate() };

			_mapper.Map<Step>(dto).Returns(createdStep);
			_idGenerator.Generate().Returns(Guid.NewGuid());
			_userContext.UserId.Returns(Guid.NewGuid());
			_dateTimeProvider.UtcNow.Returns(DateTime.UtcNow);

			//_stepRepository.InsertAsync(Arg.Any<Step>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
			_stepRepository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);
			_mapper.Map<StepDto>(createdStep).Returns(expectedDto);

			// Act
			var result = await _stepService.CreateNewStep(dto);

			// Assert
			Assert.Equal(expectedDto.Id, result.Id);

			// Kiểm tra các trường được set đúng
			Assert.NotEqual(Guid.Empty, createdStep.Id);
			Assert.Equal(_userContext.UserId.ToString(), createdStep.CreatedBy);
			Assert.Equal(_dateTimeProvider.UtcNow, createdStep.Created);
			Assert.NotNull(createdStep.ConfigSchema); // hoặc Assert.Equal(dto.ConfigSchema.GetRawText(), createdStep.ConfigSchema);

			await _stepRepository.Received(1).InsertAsync(createdStep, Arg.Any<CancellationToken>());
			await _stepRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
		} 

		[Fact]
		public async Task UpdateStep_WhenStepNotFound_ThrowsNotFoundException()
		{
			var id = _idGenerator.Generate();
			_stepRepository.GetByIdAsync(id).Returns((Step?)null);

			await Assert.ThrowsAsync<NotFoundException>(
				() => _stepService.UpdateStep(id, new StepUpdateDto()));
		}

		[Fact]
		public async Task UpdateStep_WhenStepExists_ReturnsUpdatedDto()
		{
			var id = _idGenerator.Generate();
			var step = new Step { Id = id };
			var dto = new StepUpdateDto();
			var expectedDto = new StepDto { Id = id };

			_stepRepository.GetByIdAsync(id).Returns(step);
			_userContext.UserId.Returns(Guid.NewGuid());
			_dateTimeProvider.UtcNow.Returns(DateTime.UtcNow);
			_stepRepository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);
			_mapper.Map<StepDto>(step).Returns(expectedDto);

			var result = await _stepService.UpdateStep(id, dto);

			Assert.Equal(expectedDto.Id, result.Id);
			Assert.Equal(_userContext.UserId.ToString(), step.LastModifiedBy);
			Assert.Equal(_dateTimeProvider.UtcNow, step.LastModified);
		}

		[Fact]
		public async Task DeleteStep_WhenStepNotFound_ThrowsNotFoundException()
		{
			var id = Guid.NewGuid();
			_stepRepository.GetByIdAsync(id).Returns((Step?)null);

			await Assert.ThrowsAsync<NotFoundException>(
				() => _stepService.DeleteStep(id));
		}

		[Fact]
		public async Task DeleteStep_WhenStepExists_ReturnsTrue()
		{
			var id = _idGenerator.Generate();
			var step = new Step { Id = id };

			_stepRepository.GetByIdAsync(id).Returns(step);
			_stepRepository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

			var result = await _stepService.DeleteStep(id);

			Assert.True(result);
			Assert.True(step.IsDeleted);
		}

	}
}
