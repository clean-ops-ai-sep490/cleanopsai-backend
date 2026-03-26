using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.Modules.ClientManagement.Application.Dtos;
using CleanOpsAi.Modules.ClientManagement.Application.Dtos.WorkareaDetails;
using CleanOpsAi.Modules.ClientManagement.Application.Interfaces;
using CleanOpsAi.Modules.ClientManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.ClientManagement.Application.Services
{
    public class WorkAreaDetailService : IWorkAreaDetailService
    {
        private readonly IWorkAreaDetailRepository _repository;
        private readonly IUserContext _userContext;

        public WorkAreaDetailService(IWorkAreaDetailRepository repository, IUserContext userContext)
        {
            _repository = repository;
            _userContext = userContext;
        }

        // get by id with work area name
        public async Task<WorkAreaDetailResponse?> GetByIdAsync(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);

            if (entity == null) return null;

            return new WorkAreaDetailResponse
            {
                Id = entity.Id,
                Name = entity.Name,
                Area = entity.Area,
                TotalArea = entity.TotalArea,
                WorkAreaId = entity.WorkAreaId,
                WorkAreaName = entity.WorkArea?.Name
            };
        }

        // get all with pagination and work area name
        public async Task<PagedResponse<WorkAreaDetailResponse>> GetAllPaginationAsync(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            var (items, totalCount) = await _repository.GetAllPaginationAsync(pageNumber, pageSize);

            var responses = items.Select(x => new WorkAreaDetailResponse
            {
                Id = x.Id,
                Name = x.Name,
                Area = x.Area,
                TotalArea = x.TotalArea,
                WorkAreaId = x.WorkAreaId,
                WorkAreaName = x.WorkArea?.Name
            }).ToList();

            return new PagedResponse<WorkAreaDetailResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalElements = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                Content = responses
            };
        }

        // get by work area id with pagination and work area name
        public async Task<PagedResponse<WorkAreaDetailResponse>> GetByWorkAreaIdPaginationAsync(Guid workAreaId, int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            var (items, totalCount) = await _repository
                .GetByWorkAreaIdPaginationAsync(workAreaId, pageNumber, pageSize);

            var responses = items.Select(x => new WorkAreaDetailResponse
            {
                Id = x.Id,
                Name = x.Name,
                Area = x.Area,
                TotalArea = x.TotalArea,
                WorkAreaId = x.WorkAreaId,
                WorkAreaName = x.WorkArea?.Name
            }).ToList();

            return new PagedResponse<WorkAreaDetailResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalElements = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                Content = responses
            };
        }

        // create with work area id and return with work area name
        public async Task<WorkAreaDetailResponse> CreateAsync(WorkAreaDetailCreateRequest request)
        {
            var entity = new WorkAreaDetail
            {
                Name = request.Name,
                Area = request.Area,
                TotalArea = request.TotalArea,
                WorkAreaId = request.WorkAreaId,
                Created = DateTime.UtcNow,
                CreatedBy = _userContext.UserId.ToString(),
            };

            await _repository.CreateAsync(entity);

            return new WorkAreaDetailResponse
            {
                Id = entity.Id,
                Name = entity.Name,
                Area = entity.Area,
                TotalArea = entity.TotalArea,
                WorkAreaId = entity.WorkAreaId
            };
        }

        //  update with work area id and return with work area name
        public async Task<WorkAreaDetailResponse?> UpdateAsync(Guid id, WorkAreaDetailUpdateRequest request)
        {
            var entity = await _repository.GetByIdAsync(id);

            if (entity == null) return null;

            entity.Name = request.Name;
            entity.Area = request.Area;
            entity.TotalArea = request.TotalArea;
            entity.Created = DateTime.UtcNow;
            entity.CreatedBy = _userContext.UserId.ToString();

            await _repository.UpdateAsync(entity);

            return await GetByIdAsync(id);
        }

        // delete by id and return number of affected rows
        public async Task<int> DeleteAsync(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);

            if (entity == null) return 0;

            return await _repository.DeleteAsync(entity);
        }

    }
}
