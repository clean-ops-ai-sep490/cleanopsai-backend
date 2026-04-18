using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.Modules.Workforce.Application.Dtos;
using CleanOpsAi.Modules.Workforce.Application.Dtos.Skills;
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
    public class SkillService : ISkillService
    {
        private readonly ISkillRepository _repository;
        private readonly IUserContext _userContext;
        private readonly IDateTimeProvider _dateTimeProvider;

        public SkillService(ISkillRepository repository, IUserContext userContext, IDateTimeProvider dateTimeProvider)
        {
            _repository = repository;
            _userContext = userContext;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<List<SkillResponse>?> GetByIdAsync(Guid id)
        {
            var skill = await _repository.GetByIdAsync(id);

            if (skill == null)
                return null;

            return new List<SkillResponse>
            {
                new SkillResponse
                {
                    Id = skill.Id,
                    Name = skill.Name,
                    Category = skill.Category,
                    Description = skill.Description
                }
            };
        }

        public async Task<List<SkillResponse>> GetAllAsync()
        {
            var skills = await _repository.GetAllAsync();

            return skills.Select(x => new SkillResponse
            {
                Id = x.Id,
                Name = x.Name,
                Category = x.Category,
                Description = x.Description
            }).ToList();
        }

        public async Task<PagedResponse<SkillResponse>> GetAllPaginationAsync(int pageNumber, int pageSize)
        {
            var (items, totalCount) = await _repository.GetAllPaginationAsync(pageNumber, pageSize);

            var responses = items.Select(x => new SkillResponse
            {
                Id = x.Id,
                Name = x.Name,
                Category = x.Category,
                Description = x.Description
            }).ToList();

            return new PagedResponse<SkillResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalElements = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                Content = responses
            };
        }

        public async Task<SkillResponse?> CreateAsync(SkillCreateRequest request)
        {
            var skill = new Skill
            {
                Id = Uuid7.NewGuid(),
                Name = request.Name,
                Category = request.Category,
                Description = request.Description,
                Created = _dateTimeProvider.UtcNow,
                CreatedBy = _userContext.UserId.ToString(),
                IsDeleted = false
            };

            await _repository.CreateAsync(skill);

            return new SkillResponse
            {
                Id = skill.Id,
                Name = skill.Name,
                Category = skill.Category,
                Description = skill.Description
            };
        }

        public async Task<SkillResponse?> UpdateAsync(Guid id, SkillUpdateRequest request)
        {
            var skill = await _repository.GetByIdAsync(id);

            if (skill == null)
                return null;

            skill.Name = string.IsNullOrWhiteSpace(request.Name) ? skill.Name : request.Name;
            skill.Category = string.IsNullOrWhiteSpace(request.Category) ? skill.Category : request.Category;
            skill.Description = request.Description ?? skill.Description;
            skill.LastModified = _dateTimeProvider.UtcNow;
            skill.LastModifiedBy = _userContext.UserId.ToString();

            await _repository.UpdateAsync(skill);

            return new SkillResponse
            {
                Id = skill.Id,
                Name = skill.Name,
                Category = skill.Category,
                Description = skill.Description
            };
        }

        public async Task<int> DeleteAsync(Guid id)
        {
            return await _repository.DeleteAsync(id);
        }

        public async Task<List<string>> GetAllCategoriesAsync()
        {
            return await _repository.GetAllCategoriesAsync();
        }

        public async Task<List<SkillResponse>> GetSkillsByCategoryAsync(string category)
        {
            var skills = await _repository.GetByCategoryAsync(category);

            if (string.IsNullOrWhiteSpace(category))
                return new List<SkillResponse>();

            return skills.Select(x => new SkillResponse
            {
                Id = x.Id,
                Name = x.Name,
                Category = x.Category,
                Description = x.Description
            }).ToList();
        }

        public async Task<List<WorkerSkillResponse>> GetSkillsByWorkerIdAsync(Guid workerId)
        {
            if (workerId == Guid.Empty)
                return new List<WorkerSkillResponse>();

            var workerSkills = await _repository.GetSkillsByWorkerIdAsync(workerId);

            return workerSkills.Select(ws => new WorkerSkillResponse
            {
                SkillId = ws.SkillId,
                Name = ws.Skill.Name,
                Category = ws.Skill.Category,
                Description = ws.Skill.Description,
                SkillLevel = ws.SkillLevel
            }).ToList();
        }

    }
}
