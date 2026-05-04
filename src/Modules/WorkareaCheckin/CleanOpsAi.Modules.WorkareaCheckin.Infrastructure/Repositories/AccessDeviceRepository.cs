using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.BuildingBlocks.Infrastructure.Extensions;
using CleanOpsAi.Modules.WorkareaCheckin.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.WorkareaCheckin.Application.DTOs.Response;
using CleanOpsAi.Modules.WorkareaCheckin.Domain.Entities;
using CleanOpsAi.Modules.WorkareaCheckin.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CleanOpsAi.Modules.WorkareaCheckin.Infrastructure.Repositories
{
	public class AccessDeviceRepository : BaseRepo<AccessDevice, Guid>, IAccessDeviceRepository
	{
		public AccessDeviceRepository(WorkareaCheckinDbContext context) : base(context)
		{

		}

		public async Task<AccessDevice?> GetByIdentifierAsync(string identifier, CancellationToken ct = default)
		{
			return await _context.AccessDevices.FirstOrDefaultAsync(d => d.Identifier == identifier, ct);
		}

		public async Task<AccessDevice?> GetByUuidAsync(string uuid, CancellationToken ct = default)
		{
			return await _context.AccessDevices
				.Include(x => x.BleInfo) 
				.FirstOrDefaultAsync(x => x.BleInfo != null && x.BleInfo.ServiceUuid == uuid, ct);
		}

		public async Task<PaginatedResult<AccessDevice>> GetByCheckinPointAsync(
		Guid checkinPointId,
		PaginationRequest request,
		CancellationToken ct = default)
		{
			var query = _context.AccessDevices
				.AsNoTracking()
				.Where(x => x.WorkareaCheckinPointId == checkinPointId)
				.Include(x => x.BleInfo);  

			return await query.ToPaginatedResultAsync(request, ct);
		}
	}
}