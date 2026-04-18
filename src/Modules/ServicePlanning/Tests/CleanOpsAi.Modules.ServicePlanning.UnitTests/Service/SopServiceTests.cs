using AutoMapper;
using CleanOpsAi.BuildingBlocks.Application;
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
	public class SopServiceTests
	{
		private readonly ISopRepository _sopRepository;
		private readonly IStepRepository _stepRepository;
		private readonly ISopRequiredSkillRepository _sopRequiredSkillRepository;
		private readonly ISopRequiredCertificationRepository _sopRequiredCertificationRepository;
		private readonly IMapper _mapper;
		private readonly IDateTimeProvider _dateTimeProvider;
		private readonly IIdGenerator _idGenerator;
		private readonly IUserContext _userContext;
		private readonly SopService _service;

		public SopServiceTests()
		{
			_sopRepository = Substitute.For<ISopRepository>();
			_stepRepository = Substitute.For<IStepRepository>();
			_sopRequiredSkillRepository = Substitute.For<ISopRequiredSkillRepository>();
			_sopRequiredCertificationRepository = Substitute.For<ISopRequiredCertificationRepository>();
			_mapper = Substitute.For<IMapper>();
			_dateTimeProvider = Substitute.For<IDateTimeProvider>();
			_idGenerator = Substitute.For<IIdGenerator>();
			_userContext = Substitute.For<IUserContext>();

			_service = new SopService(
				_sopRepository,
				_stepRepository,
				_sopRequiredSkillRepository,
				_sopRequiredCertificationRepository,
				_mapper,
				_dateTimeProvider,
				_idGenerator,
				_userContext);
		}

		[Fact]
		public async Task CreateSopAsync_Should_ValidateAndCreateSop()
		{
			var dto = new SopCreateDto
			{
				Name = "Test SOP",
				Steps = new List<SopStepCreateDto>
				{
					new SopStepCreateDto { StepId = Guid.NewGuid(), StepOrder = 1, ConfigDetail = JsonDocument.Parse("{}").RootElement }
				}
			};

			var step = new Step { Id = dto.Steps[0].StepId, ConfigSchema = "{}" };
			var sop = new Sop();
			var expectedDto = new SopDto { Id = Guid.NewGuid() };

			_stepRepository.GetByIdsAsync(Arg.Any<List<Guid>>()).Returns(new List<Step> { step });
			_mapper.Map<Sop>(dto).Returns(sop);
			_idGenerator.Generate().Returns(expectedDto.Id);
			_userContext.UserId.Returns(Guid.NewGuid());
			_dateTimeProvider.UtcNow.Returns(DateTime.UtcNow);
			//_sopRepository.InsertAsync(sop, Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
			_sopRepository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);
			_sopRepository.GetByIdWithStepsAsync(expectedDto.Id).Returns(sop);
			_mapper.Map<SopDto>(sop).Returns(expectedDto);

			var result = await _service.CreateSopAsync(dto);

			Assert.Equal(expectedDto.Id, result.Id);
		}

		[Fact]
		public async Task GetSopByIdAsync_WhenSopExists_ReturnsDto()
		{
			var id = Guid.NewGuid();
			var sop = new Sop { Id = id };
			var expectedDto = new SopDto { Id = id };

			_sopRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(sop);
			_mapper.Map<SopDto>(sop).Returns(expectedDto);

			var result = await _service.GetSopByIdAsync(id);

			Assert.Equal(expectedDto.Id, result.Id);
		}

		[Fact]
		public async Task GetSopByIdAsync_WhenSopNotFound_ReturnsNull()
		{
			var id = Guid.NewGuid();
			_sopRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Sop?)null);

			var result = await _service.GetSopByIdAsync(id);

			Assert.Null(result);
		}

		[Fact]
		public async Task UpdateSopAsync_WhenSopNotFound_ReturnsNull()
		{
			var id = Guid.NewGuid();
			var dto = new SopUpdateDto { Name = "Updated" };

			_sopRepository.GetByIdWithStepsAsync(id, true).Returns((Sop?)null);

			var result = await _service.UpdateSopAsync(id, dto);

			Assert.Null(result);
		}

		[Fact]
		public async Task DeleteSopAsync_WhenSopNotFound_ReturnsFalse()
		{
			var id = Guid.NewGuid();
			_sopRepository.GetByIdWithStepsAsync(id).Returns((Sop?)null);

			var result = await _service.DeleteSopAsync(id);

			Assert.False(result);
		}

		[Fact]
		public async Task DeleteSopAsync_WhenSopExists_SetsIsDeletedAndReturnsTrue()
		{
			var id = Guid.NewGuid();
			var sop = new Sop { Id = id, SopSteps = new List<SopStep>() };
			_sopRepository.GetByIdWithStepsAsync(id).Returns(sop);
			_sopRepository.SaveChangesAsync().Returns(1);

			var result = await _service.DeleteSopAsync(id);

			Assert.True(result);
			Assert.True(sop.IsDeleted);
		}

		[Fact]
		public async Task Gets_Should_ReturnPaginatedResult()
		{
			var request = new PaginationRequest { PageNumber = 1, PageSize = 10 };
			var query = new GetsSopQueryFilter(); 

			var sops = new List<Sop> { new Sop(), new Sop() };
			var sopDtos = new List<SopListDto> { new SopListDto(), new SopListDto() };

			var pagedResult = new PaginatedResult<Sop>(1, 10, 2, sops);

			_sopRepository
				.GetsPaging(Arg.Any<GetsSopQueryFilter>(), request, Arg.Any<CancellationToken>())
				.Returns(pagedResult);

			_mapper
				.Map<List<SopListDto>>(Arg.Any<List<Sop>>())
				.Returns(sopDtos);

			var result = await _service.Gets(query, request); 

			Assert.Equal(2, result.TotalElements);
			Assert.Equal(2, result.Content.Count);
		}
	}
}
