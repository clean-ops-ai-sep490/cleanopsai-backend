using CleanOpsAi.BuildingBlocks.Application.Exceptions;
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
        private readonly IWorkerRepository _workerRepository;
        private readonly ISkillRepository _skillRepository;

        public WorkerSkillService(IWorkerSkillRepository repository, IWorkerRepository workerRepository, ISkillRepository skillRepository)
        {
            _repository = repository;
            _workerRepository = workerRepository;
            _skillRepository = skillRepository;
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

        public async Task<WorkerSkillResponse> CreateAsync(WorkerSkillCreateRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // validate input
            if (request.WorkerId == Guid.Empty || request.SkillId == Guid.Empty)
                throw new Exception("WorkerId hoặc SkillId không hợp lệ");

            // check worker tồn tại
            var worker = await _workerRepository.GetByIdAsync(request.WorkerId);
            if (worker == null)
                throw new Exception("Worker không tồn tại");

            // check skill tồn tại
            var skill = await _skillRepository.GetByIdAsync(request.SkillId);
            if (skill == null)
                throw new Exception("Skill không tồn tại");

            var existing = await _repository.GetByIdAsync(
                request.WorkerId,
                request.SkillId
            );

            if (existing != null)
                throw new BadRequestException("Worker đã có skill này rồi");

            var entity = new WorkerSkill
            {
                WorkerId = request.WorkerId,
                SkillId = request.SkillId,
                SkillLevel = request.SkillLevel
            };

            await _repository.CreateAsync(entity);

            // tránh null reference ở đây
            return new WorkerSkillResponse
            {
                WorkerId = worker.Id,
                WorkerName = worker.FullName, // nhớ check null nếu nullable
                SkillId = skill.Id,
                SkillName = skill.Name,
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
