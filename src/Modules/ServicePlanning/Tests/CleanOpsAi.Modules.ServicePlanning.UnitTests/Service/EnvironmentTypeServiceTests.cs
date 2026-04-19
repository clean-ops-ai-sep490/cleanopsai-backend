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

namespace CleanOpsAi.Modules.ServicePlanning.UnitTests.Service
{
	public class EnvironmentTypeServiceTests
	{
		private readonly IEnvironmentTypeRepository _environmentTypeRepository;
		private readonly IMapper _mapper;
		private readonly EnvironmentTypeService _service;
		private readonly IDateTimeProvider _dateTimeProvider;
		private readonly IIdGenerator _idGenerator;
		private readonly IUserContext _userContext;

		private readonly Guid _userId = Guid.NewGuid();

		public EnvironmentTypeServiceTests()
		{
			_environmentTypeRepository = Substitute.For<IEnvironmentTypeRepository>();
			_mapper = Substitute.For<IMapper>();
			_dateTimeProvider = Substitute.For<IDateTimeProvider>();
			_idGenerator = Substitute.For<IIdGenerator>();
			_userContext = Substitute.For<IUserContext>();

			_userContext.UserId.Returns(_userId);

			_service = new EnvironmentTypeService(
				_environmentTypeRepository,
				_mapper,
				_userContext,
				_dateTimeProvider,
				_idGenerator
				);
		}

		[Fact]
		public async Task Create_Should_InsertEntity_And_ReturnMappedDto()
		{
			var dto = new EnvironmentTypeCreateDto
			{
				Name = "Test Environment",
				Description = "Test description"
			};

			var entity = new EnvironmentType();
			var expectedId = Guid.NewGuid();
			var expectedCreated = DateTime.UtcNow;
			var expectedDto = new EnvironmentTypeDto { Id = expectedId, Name = dto.Name, Description = dto.Description };

			_mapper.Map<EnvironmentType>(dto).Returns(entity);
			_idGenerator.Generate().Returns(expectedId);
			_dateTimeProvider.UtcNow.Returns(expectedCreated);
			_environmentTypeRepository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);
			_mapper.Map<EnvironmentTypeDto>(entity).Returns(expectedDto);

			var result = await _service.Create(dto);

			Assert.Equal(expectedDto.Id, result.Id);
			Assert.Equal(dto.Name, result.Name);
			Assert.Equal(dto.Description, result.Description);
			Assert.Equal(expectedId, entity.Id);
			Assert.Equal(expectedCreated, entity.Created);
			Assert.Equal(_userId.ToString(), entity.CreatedBy);

			await _environmentTypeRepository.Received(1).InsertAsync(entity, Arg.Any<CancellationToken>());
			await _environmentTypeRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task GetById_WhenEnvironmentTypeExists_ReturnsDto()
		{
			var id = Guid.NewGuid();
			var entity = new EnvironmentType { Id = id, Name = "Env A", Description = "Desc" };
			var expectedDto = new EnvironmentTypeDto { Id = id, Name = entity.Name, Description = entity.Description };

			_environmentTypeRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(entity);
			_mapper.Map<EnvironmentTypeDto>(entity).Returns(expectedDto);

			var result = await _service.GetById(id);

			Assert.Equal(expectedDto.Id, result.Id);
			Assert.Equal(expectedDto.Name, result.Name);
			Assert.Equal(expectedDto.Description, result.Description);
		}

		[Fact]
		public async Task GetById_WhenEnvironmentTypeNotFound_ThrowsNotFoundException()
		{
			var id = Guid.NewGuid();
			_environmentTypeRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((EnvironmentType?)null);

			await Assert.ThrowsAsync<NotFoundException>(() => _service.GetById(id));
		}

		[Fact]
		public async Task Gets_Should_ReturnPaginatedResult()
		{
			var request = new PaginationRequest { PageNumber = 1, PageSize = 10 };
			var entities = new List<EnvironmentType>
			{
				new EnvironmentType { Id = Guid.NewGuid(), Name = "Env 1" },
				new EnvironmentType { Id = Guid.NewGuid(), Name = "Env 2" }
			};
			var mapped = new List<EnvironmentTypeDto>
			{
				new EnvironmentTypeDto { Id = entities[0].Id, Name = entities[0].Name },
				new EnvironmentTypeDto { Id = entities[1].Id, Name = entities[1].Name }
			};

			var page = new PaginatedResult<EnvironmentType>(1, 10, 2, entities);

			_environmentTypeRepository.GetsPaging(request, Arg.Any<CancellationToken>()).Returns(page);
			_mapper.Map<List<EnvironmentTypeDto>>(Arg.Any<List<EnvironmentType>>()).Returns(mapped);

			var result = await _service.Gets(request);

			Assert.Equal(2, result.TotalElements);
			Assert.Equal(2, result.Content.Count);
			Assert.Equal(mapped, result.Content);
		}

		[Fact]
		public async Task Update_WhenEnvironmentTypeNotFound_ThrowsNotFoundException()
		{
			var id = Guid.NewGuid();
			var dto = new EnvironmentTypeUpdateDto { Name = "Updated", Description = "Updated desc" };

			_environmentTypeRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((EnvironmentType?)null);

			await Assert.ThrowsAsync<NotFoundException>(() => _service.Update(id, dto));
		}

		[Fact]
		public async Task Update_WhenEnvironmentTypeExists_MapsAndSaves_ReturnsDto()
		{
			var id = Guid.NewGuid();

			var entity = new EnvironmentType
			{
				Id = id,
				Name = "Original",
				Description = "Original desc"
			};

			var dto = new EnvironmentTypeUpdateDto
			{
				Name = "Updated",
				Description = "Updated desc"
			};

			var expectedDto = new EnvironmentTypeDto
			{
				Id = id,
				Name = dto.Name,
				Description = dto.Description
			};
			 
			var now = DateTime.UtcNow;
			_dateTimeProvider.UtcNow.Returns(now);

			_environmentTypeRepository
				.GetByIdAsync(id, Arg.Any<CancellationToken>())
				.Returns(entity);

			_environmentTypeRepository
				.SaveChangesAsync(Arg.Any<CancellationToken>())
				.Returns(1);

			_mapper
				.Map<EnvironmentTypeDto>(entity)
				.Returns(expectedDto);

			var result = await _service.Update(id, dto);

			// assert result
			Assert.Equal(expectedDto.Id, result.Id);
			Assert.Equal(expectedDto.Name, result.Name);
			Assert.Equal(expectedDto.Description, result.Description);

			// 🔥 assert chuẩn hơn
			Assert.Equal(now, entity.LastModified);
			Assert.Equal(_userId.ToString(), entity.LastModifiedBy);

			await _environmentTypeRepository.Received(1)
				.SaveChangesAsync(Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task Delete_WhenEnvironmentTypeNotFound_ThrowsNotFoundException()
		{
			var id = Guid.NewGuid();
			_environmentTypeRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((EnvironmentType?)null);

			await Assert.ThrowsAsync<NotFoundException>(() => _service.Delete(id));
		}

		[Fact]
		public async Task Delete_WhenEnvironmentTypeExists_SetsIsDeletedAndReturnsTrue()
		{
			var id = Guid.NewGuid();
			var entity = new EnvironmentType { Id = id, Name = "Env", Description = "Desc" };
			_environmentTypeRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(entity);
			_environmentTypeRepository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

			var result = await _service.Delete(id);

			Assert.True(result);
			Assert.True(entity.IsDeleted);
			await _environmentTypeRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
		}
	}
}
