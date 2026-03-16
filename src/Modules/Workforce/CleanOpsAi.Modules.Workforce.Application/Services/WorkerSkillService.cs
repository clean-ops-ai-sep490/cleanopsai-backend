using CleanOpsAi.Modules.Workforce.Application.Dtos;
using CleanOpsAi.Modules.Workforce.Application.Dtos.WorkerSkills;
using CleanOpsAi.Modules.Workforce.Application.Interfaces;
using CleanOpsAi.Modules.Workforce.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.Workforce.Application.Services
{
    public class WorkerSkillService : IWorkerSkillService
    {
        private readonly IWorkerSkillRepository _repository;

        public WorkerSkillService(IWorkerSkillRepository repository)
        {
            _repository = repository;
        }

        public async Task<WorkerSkillResponse?> GetByIdAsync(Guid workerId, Guid skillId)
        {
            var entity = await _repository.GetByIdAsync(workerId, skillId);

            if (entity == null)
                return null;

            return new WorkerSkillResponse
            {
                WorkerId = entity.WorkerId,
                SkillId = entity.SkillId,
                WorkerName = entity.Worker.FullName,
                SkillName = entity.Skill.Name,
                SkillLevel = entity.SkillLevel
            };
        }

        public async Task<List<WorkerSkillResponse>> GetAllAsync()
        {
            var items = await _repository.GetAllAsync();

            return items.Select(x => new WorkerSkillResponse
            {
                WorkerId = x.WorkerId,
                SkillId = x.SkillId,
                WorkerName = x.Worker.FullName,
                SkillName = x.Skill.Name,
                SkillLevel = x.SkillLevel
            }).ToList();
        }

        public async Task<PagedResponse<WorkerSkillResponse>> GetAllPaginationAsync(int pageNumber, int pageSize)
        {
            var (items, totalCount) = await _repository.GetAllPaginationAsync(pageNumber, pageSize);

            var responses = items.Select(x => new WorkerSkillResponse
            {
                WorkerId = x.WorkerId,
                SkillId = x.SkillId,
                WorkerName = x.Worker.FullName,
                SkillName = x.Skill.Name,
                SkillLevel = x.SkillLevel
            }).ToList();

            return new PagedResponse<WorkerSkillResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalElements = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                Content = responses
            };
        }

        public async Task<WorkerSkillResponse?> CreateAsync(WorkerSkillCreateRequest request)
        {
            var entity = new WorkerSkill
            {
                WorkerId = request.WorkerId,
                SkillId = request.SkillId,
                SkillLevel = request.SkillLevel
            };

            await _repository.CreateAsync(entity);

            return new WorkerSkillResponse
            {
                WorkerId = entity.WorkerId,
                SkillId = entity.SkillId,
                WorkerName = entity.Worker.FullName,
                SkillName = entity.Skill.Name,
                SkillLevel = entity.SkillLevel
            };
        }

        public async Task<WorkerSkillResponse?> UpdateAsync(Guid workerId, Guid skillId, WorkerSkillUpdateRequest request)
        {
            var entity = await _repository.GetByIdAsync(workerId, skillId);

            if (entity == null)
                return null;

            entity.SkillLevel = request.SkillLevel;

            await _repository.UpdateAsync(entity);

            return new WorkerSkillResponse
            {
                WorkerId = entity.WorkerId,
                SkillId = entity.SkillId,
                WorkerName = entity.Worker.FullName,
                SkillName = entity.Skill.Name,
                SkillLevel = entity.SkillLevel
            };
        }

        public async Task<int> DeleteAsync(Guid workerId, Guid skillId)
        {
            var entity = await _repository.GetByIdAsync(workerId, skillId);

            if (entity == null)
                return 0;

            return await _repository.DeleteAsync(workerId, skillId);
        }
    }
}
