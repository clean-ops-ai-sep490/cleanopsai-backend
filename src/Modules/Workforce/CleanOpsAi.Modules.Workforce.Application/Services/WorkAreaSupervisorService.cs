using CleanOpsAi.BuildingBlocks.Application;
using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request;
using CleanOpsAi.Modules.Workforce.Application.Dtos;
using CleanOpsAi.Modules.Workforce.Application.Dtos.WorkAreaSupervisors;
using CleanOpsAi.Modules.Workforce.Application.Interfaces;
using CleanOpsAi.Modules.Workforce.Domain.Entities;
using MassTransit;
using Medo; 

namespace CleanOpsAi.Modules.Workforce.Application.Services
{
    public class WorkAreaSupervisorService : IWorkAreaSupervisorService
    {
        private readonly IWorkAreaSupervisorRepository _repository;
        private readonly IUserContext _userContext;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IRequestClient<GetWorkAreasByIdsRequest> _client;

        public WorkAreaSupervisorService(IWorkAreaSupervisorRepository repository, IUserContext userContext, IDateTimeProvider dateTimeProvider, IRequestClient<GetWorkAreasByIdsRequest> client)
        {
            _repository = repository;
            _userContext = userContext;
            _dateTimeProvider = dateTimeProvider;
            _client = client;
        }

        public async Task<WorkAreaSupervisorResponse?> GetByIdAsync(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);

            if (entity == null)
                return null;

            return new WorkAreaSupervisorResponse
            {
                Id = entity.Id,
                WorkAreaId = entity.WorkAreaId,
                WorkerId = entity.WorkerId,
                WorkerName = entity.Worker?.FullName,
                SupervisorId = entity.UserId,
                Created = entity.Created
            };
        }

        public async Task<List<WorkAreaSupervisorResponse>> GetByUserIdAsync(Guid userId)
        {
            var items = await _repository.GetByUserIdAsync(userId);

            return items.Select(entity => new WorkAreaSupervisorResponse
            {
                Id = entity.Id,
                WorkAreaId = entity.WorkAreaId,
                WorkerId = entity.WorkerId,
                WorkerName = entity.Worker?.FullName,
                SupervisorId = entity.UserId,
                Created = entity.Created
            }).ToList();
        }

        public async Task<PagedResponse<WorkAreaSupervisorResponse>> GetByWorkerIdPaginationAsync(
            Guid workerId,
            int pageNumber,
            int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            var (items, totalCount) = await _repository.GetByWorkerIdPaginationAsync(workerId, pageNumber, pageSize);

            var responses = items.Select(entity => new WorkAreaSupervisorResponse
            {
                Id = entity.Id,
                WorkAreaId = entity.WorkAreaId,
                WorkerId = entity.WorkerId,
                WorkerName = entity.Worker?.FullName,
                SupervisorId = entity.UserId,
                Created = entity.Created
            }).ToList();

            return new PagedResponse<WorkAreaSupervisorResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalElements = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                Content = responses
            };
        }

        public async Task<List<WorkAreaSupervisorResponse>> GetAllAsync()
        {
            var items = await _repository.GetAllAsync();

            return items.Select(entity => new WorkAreaSupervisorResponse
            {
                Id = entity.Id,
                WorkAreaId = entity.WorkAreaId,
                WorkerId = entity.WorkerId,
                WorkerName = entity.Worker?.FullName,
                SupervisorId = entity.UserId,
                Created = entity.Created
            }).ToList();
        }

        public async Task<PagedResponse<WorkAreaSupervisorResponse>> GetAllPaginationAsync(int pageNumber, int pageSize)
        {
            var (items, totalCount) = await _repository.GetAllPaginationAsync(pageNumber, pageSize);

            var responses = items.Select(entity => new WorkAreaSupervisorResponse
            {
                Id = entity.Id,
                WorkAreaId = entity.WorkAreaId,
                WorkerId = entity.WorkerId,
                WorkerName = entity.Worker?.FullName,
                SupervisorId = entity.UserId,
                Created = entity.Created
            }).ToList();

            return new PagedResponse<WorkAreaSupervisorResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalElements = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                Content = responses
            };
        }

        public async Task<List<WorkAreaSupervisorResponse>> GetByWorkAreaIdAsync(Guid workAreaId)
        {
            var items = await _repository.GetByWorkAreaIdAsync(workAreaId);

            return items.Select(entity => new WorkAreaSupervisorResponse
            {
                Id = entity.Id,
                WorkAreaId = entity.WorkAreaId,
                WorkerId = entity.WorkerId,
                WorkerName = entity.Worker?.FullName,
                SupervisorId = entity.UserId,
                Created = entity.Created
            }).ToList();
        }



        public async Task<WorkAreaSupervisorAssignResponse> UpdateAsync(WorkAreaSupervisorUpdateRequest request)
        {
            var existing = await _repository.GetByWorkAreaIdAsync(request.WorkAreaId);

            if (!existing.Any())
                throw new InvalidOperationException("WorkArea chưa có supervisor để update.");

            // update supervisor cho toàn bộ worker
            await _repository.UpdateSupervisorAsync(
                request.WorkAreaId,
                request.SupervisorId);

            var updated = await _repository.GetByWorkAreaIdAsync(request.WorkAreaId);

            return new WorkAreaSupervisorAssignResponse
            {
                WorkAreaId = request.WorkAreaId,
                SupervisorId = request.SupervisorId,
                TotalAssigned = updated.Count,
                Assignments = updated.Select(x => new WorkAreaSupervisorResponse
                {
                    Id = x.Id,
                    WorkAreaId = x.WorkAreaId,
                    WorkerId = x.WorkerId,
                    WorkerName = x.Worker?.FullName,
                    SupervisorId = x.UserId,
                    Created = x.Created
                }).ToList()
            };
        }

        public async Task<int> DeleteAsync(Guid id)
        {
            return await _repository.DeleteAsync(id);
        }

        // Gps

        public async Task<PagedResponse<WorkerLiveGpsResponse>> GetWorkersLiveStatusByWorkAreaPagingAsync(
            Guid workAreaId,
            int pageNumber,
            int pageSize,
            int offlineThresholdMinutes)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            var items = await _repository.GetLatestGpsByWorkAreaAsync(workAreaId);

            var now = _dateTimeProvider.UtcNow;
            var threshold = now.AddMinutes(-offlineThresholdMinutes);

            var mapped = items.Select(x => new WorkerLiveGpsResponse
            {
                WorkerId = x.WorkerId,
                WorkerName = x.Worker?.FullName,
                Latitude = x.Latitude,
                Longitude = x.Longitude,

                IsConfirmed = x.IsConfirmed,

                LastSeen = x.Created,

                // KEY LOGIC
                IsOnline = x.Created >= threshold
            })
            .OrderByDescending(x => x.LastSeen)
            .ToList();

            var total = mapped.Count;

            var paged = mapped
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedResponse<WorkerLiveGpsResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalElements = total,
                TotalPages = (int)Math.Ceiling((double)total / pageSize),
                Content = paged
            };
        }

        public async Task<WorkAreaSupervisorAssignResponse> AssignWorkersAsync(
    WorkAreaSupervisorAssignRequest request)
        {
            // 1. Validate supervisor unique
            var existing = await _repository.GetByWorkAreaIdAsync(request.WorkAreaId);

            if (existing.Any() && existing.Any(x => x.UserId != request.SupervisorId))
            {
                throw new InvalidOperationException("WorkArea đã có supervisor khác.");
            }

            var toCreate = new List<WorkAreaSupervisor>();

            // 2. workerIds RỖNG → vẫn phải lưu supervisor
            if (request.WorkerIds == null || !request.WorkerIds.Any())
            {
                // check đã có record nào chưa
                var hasAny = existing.Any();

                if (!hasAny)
                {
                    toCreate.Add(new WorkAreaSupervisor
                    {
                        Id = Uuid7.NewGuid(),
                        WorkAreaId = request.WorkAreaId,
                        WorkerId = null, // DB phải cho phép null
                        UserId = request.SupervisorId,
                        Created = _dateTimeProvider.UtcNow,
                        CreatedBy = _userContext.UserId.ToString(),
                        IsDeleted = false
                    });
                }
            }
            else
            {
                // 3. CASE: có worker → add bình thường
                foreach (var workerId in request.WorkerIds)
                {
                    var exists = await _repository.ExistsAsync(
                        request.WorkAreaId, request.SupervisorId, workerId);

                    if (!exists)
                    {
                        toCreate.Add(new WorkAreaSupervisor
                        {
                            Id = Uuid7.NewGuid(),
                            WorkAreaId = request.WorkAreaId,
                            WorkerId = workerId,
                            UserId = request.SupervisorId,
                            Created = _dateTimeProvider.UtcNow,
                            CreatedBy = _userContext.UserId.ToString(),
                            IsDeleted = false
                        });
                    }
                }
            }

            if (toCreate.Any())
                await _repository.CreateRangeAsync(toCreate);

            var all = await _repository.GetByWorkAreaIdAsync(request.WorkAreaId);

            return new WorkAreaSupervisorAssignResponse
            {
                WorkAreaId = request.WorkAreaId,
                SupervisorId = request.SupervisorId,
                TotalAssigned = all.Count(x => x.WorkerId != null), // chỉ đếm worker thật
                Assignments = all
                    .Where(x => x.WorkerId != null) // bỏ record null ra response
                    .Select(x => new WorkAreaSupervisorResponse
                    {
                        Id = x.Id,
                        WorkAreaId = x.WorkAreaId,
                        WorkerId = x.WorkerId,
                        WorkerName = x.Worker?.FullName,
                        SupervisorId = x.UserId,
                        Created = x.Created
                    }).ToList()
            };
        }

        // Service — UnassignWorkerAsync
        public async Task<int> UnassignWorkerAsync(Guid workAreaId, Guid userId, Guid workerId)
        {
            var entity = await _repository.GetByWorkAreaUserWorkerAsync(
                workAreaId, userId, workerId);

            if (entity == null)
                throw new KeyNotFoundException("Assignment không tồn tại.");

            return await _repository.DeleteAsync(entity.Id);
        }

        //Lấy supervisor của một worker trong một work area cụ thể 
        //public async Task<WorkAreaSupervisorResponse?> GetSupervisorByWorkAreaAndWorkerAsync(Guid workAreaId, Guid workerId)
        //{
        //    var entity = await _repository.GetByWorkAreaAndWorkerAsync(workAreaId, workerId);

        //    if (entity == null)
        //        return null;

        //    return new WorkAreaSupervisorResponse
        //    {
        //        Id = entity.Id,
        //        WorkAreaId = entity.WorkAreaId,
        //        WorkerId = entity.WorkerId,
        //        WorkerName = entity.Worker?.FullName,
        //        SupervisorId = entity.UserId,
        //        Created = entity.Created
        //    };
        //}


        public async Task<(bool Found, Guid? SupervisorUserId)> GetCommonSupervisorAsync(Guid workAreaId, Guid workerId, Guid workerIdTarget, CancellationToken ct = default)
		{ 
			var supervisorsA = await _repository.GetSupervisorIdsAsync(workAreaId, workerId, ct);
			if (!supervisorsA.Any()) return (false, null);
             
			var supervisorsB = await _repository.GetSupervisorIdsAsync(workAreaId, workerIdTarget, ct);
			if (!supervisorsB.Any()) return (false, null);
             
			var commonId = supervisorsA.Intersect(supervisorsB).FirstOrDefault();

			if (commonId != default)
			{
				return (true, commonId);
			}

			return (false, null);
		}

        public async Task<PagedResponse<WorkAreaWithLocationResponse>>GetWorkAreasBySupervisorPaginationAsync(Guid supervisorId, int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            // B1: lấy assignment
            var assignments = await _repository.GetByUserIdAsync(supervisorId);

            var workAreaIds = assignments
                .Where(x => x.WorkAreaId.HasValue)
                .Select(x => x.WorkAreaId.Value)
                .Distinct()
                .ToList();

            if (!workAreaIds.Any())
            {
                return new PagedResponse<WorkAreaWithLocationResponse>
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalElements = 0,
                    TotalPages = 0,
                    Content = new List<WorkAreaWithLocationResponse>()
                };
            }

            // B2: gọi RabbitMQ
            var response = await _client.GetResponse<GetWorkAreasByIdsResponse>(
                new GetWorkAreasByIdsRequest
                {
                    WorkAreaIds = workAreaIds
                });

            var data = response.Message.Items;

            // B3: map
            var mapped = data.Select(x => new WorkAreaWithLocationResponse
            {
                WorkAreaId = x.WorkAreaId,
                WorkAreaName = x.WorkAreaName,
                ZoneName = x.ZoneName,
                LocationName = x.LocationName,
                DisplayLocation = x.DisplayLocation
            }).ToList();

            //  B4: PAGINATION (QUAN TRỌNG)
            var totalCount = mapped.Count;

            var pagedItems = mapped
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedResponse<WorkAreaWithLocationResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalElements = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                Content = pagedItems
            };
        }

        public async Task<PagedResponse<WorkerGroupResponse>> GetUniqueWorkersBySupervisorPagingAsync(
            Guid supervisorId,
            int pageNumber,
            int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            var items = await _repository.GetWorkersBySupervisorIdAsync(supervisorId);

            // 1: group unique
            var grouped = items
                .GroupBy(x => x.WorkerId)
                .Select(g => new WorkerGroupResponse
                {
                    WorkerId = g.Key,
                    WorkerName = g.First().Worker?.FullName,
                    WorkAreaIds = g
                        .Where(x => x.WorkAreaId.HasValue)
                        .Select(x => x.WorkAreaId.Value)
                        .Distinct()
                        .ToList()
                })
                .ToList();

            // 2: total sau khi unique
            var totalCount = grouped.Count;

            // 3: paging trên memory (an toàn)
            var paged = grouped
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedResponse<WorkerGroupResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalElements = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                Content = paged
            };
        }

        public async Task<List<Guid>> GetManagedWorkerUserIdsBySupervisorAsync(Guid supervisorId, CancellationToken ct = default)
        {
            var items = await _repository.GetWorkersBySupervisorIdAsync(supervisorId);

            return items
                .Where(x => x.Worker is not null && x.Worker.IsDeleted == false)
                .Select(x => x.Worker.UserId)
                .Distinct()
                .ToList();
        }

        public async Task<PagedResponse<WorkAreaSupervisorResponse>> GetWorkersByWorkAreaPagingAsync(
            Guid workAreaId,
            int pageNumber,
            int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            var (items, totalCount) =
                await _repository.GetWorkersByWorkAreaPagingAsync(
                    workAreaId,
                    pageNumber,
                    pageSize);

            var content = items.Select(x => new WorkAreaSupervisorResponse
            {
                Id = x.Id,
                WorkAreaId = x.WorkAreaId,
                WorkerId = x.WorkerId,
                WorkerName = x.Worker?.FullName,
                SupervisorId = x.UserId,
                Created = x.Created
            }).ToList();

            return new PagedResponse<WorkAreaSupervisorResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalElements = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                Content = content
            };
        }

    } 
}
