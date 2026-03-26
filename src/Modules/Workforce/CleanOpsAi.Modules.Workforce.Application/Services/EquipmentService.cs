using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.Modules.Workforce.Application.Dtos;
using CleanOpsAi.Modules.Workforce.Application.Dtos.Equipments;
using CleanOpsAi.Modules.Workforce.Application.Interfaces;
using CleanOpsAi.Modules.Workforce.Domain.Entities;
using Medo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.Workforce.Application.Services
{
    public class EquipmentService : IEquipmentService
    {
        private readonly IEquipmentRepository _repository;
        private readonly IUserContext _userContext;

        public EquipmentService(IEquipmentRepository repository, IUserContext userContext)
        {
            _repository = repository;
            _userContext = userContext;
        }

        public async Task<List<EquipmentResponse>?> GetByIdAsync(Guid id)
        {
            var equipment = await _repository.GetByIdAsync(id);

            if (equipment == null)
                return null;

            return new List<EquipmentResponse>
            {
                new EquipmentResponse
                {
                    Id = equipment.Id,
                    Name = equipment.Name,
                    Type = equipment.Type,
                    Description = equipment.Description
                }
            };
        }

        public async Task<List<EquipmentResponse>> GetAllAsync()
        {
            var equipments = await _repository.GetAllAsync();

            return equipments.Select(x => new EquipmentResponse
            {
                Id = x.Id,
                Name = x.Name,
                Type = x.Type,
                Description = x.Description
            }).ToList();
        }

        public async Task<PagedResponse<EquipmentResponse>> GetAllPaginationAsync(int pageNumber, int pageSize)
        {
            var (items, totalCount) = await _repository.GetAllPaginationAsync(pageNumber, pageSize);

            var responses = items.Select(x => new EquipmentResponse
            {
                Id = x.Id,
                Name = x.Name,
                Type = x.Type,
                Description = x.Description
            }).ToList();

            return new PagedResponse<EquipmentResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalElements = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                Content = responses
            };
        }

        public async Task<EquipmentResponse?> CreateAsync(EquipmentCreateRequest request)
        {
            var equipment = new Equipment
            {
                Id = Uuid7.NewGuid(),
                Name = request.Name,
                Type = request.Type,
                Description = request.Description,
                Created = DateTime.UtcNow,
                CreatedBy = _userContext.UserId.ToString(),
                IsDeleted = false
            };

            await _repository.CreateAsync(equipment);

            return new EquipmentResponse
            {
                Id = equipment.Id,
                Name = equipment.Name,
                Type = equipment.Type,
                Description = equipment.Description
            };
        }

        public async Task<EquipmentResponse?> UpdateAsync(Guid id, EquipmentUpdateRequest request)
        {
            var equipment = await _repository.GetByIdAsync(id);

            if (equipment == null)
                return null;

            equipment.Name = string.IsNullOrWhiteSpace(request.Name) ? equipment.Name : request.Name;

            if (request.Type.HasValue)
                equipment.Type = request.Type.Value;

            equipment.Description = request.Description ?? equipment.Description;

            equipment.LastModified = DateTime.UtcNow;
            equipment.LastModifiedBy = _userContext.UserId.ToString();

            await _repository.UpdateAsync(equipment);

            return new EquipmentResponse
            {
                Id = equipment.Id,
                Name = equipment.Name,
                Type = equipment.Type,
                Description = equipment.Description
            };
        }

        public async Task<int> DeleteAsync(Guid id)
        {
            return await _repository.DeleteAsync(id);
        }
    }
}
