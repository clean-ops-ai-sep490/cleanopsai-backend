using CleanOpsAi.BuildingBlocks.Application.Pagination;
using CleanOpsAi.BuildingBlocks.Domain.Dtos.Sops;
using CleanOpsAi.BuildingBlocks.Infrastructure.Extensions;
using CleanOpsAi.Modules.ServicePlanning.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.ServicePlanning.Application.DTOs.Request;
using CleanOpsAi.Modules.ServicePlanning.Domain.Entities;
using CleanOpsAi.Modules.ServicePlanning.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CleanOpsAi.Modules.ServicePlanning.Infrastructure.Repositories
{
	public class SopRepository : BaseRepo<Sop, Guid>, ISopRepository
	{
		public SopRepository(ServicePlanningDbContext context) : base(context)
		{
		}

		public async Task<Sop?> GetByIdWithStepsAsync(
			Guid id,
			bool includeDeleted = false,
			CancellationToken cancellationToken = default)
		{
			var query = _context.Sops.AsQueryable();

			if (includeDeleted)
			{ 
				query = query.IgnoreQueryFilters();
			}

			return await query
				.Where(x => x.Id == id)
				.Include(x => x.SopSteps)
				.Include(x => x.SopRequiredSkills)
				.Include(x => x.SopRequiredCertifications)
				.FirstOrDefaultAsync(cancellationToken);
		}

		public async Task<Sop?> GetSopWithDetail(Guid id, CancellationToken ct = default)
		{
			var sop = await _context.Sops 
				.Include(x => x.SopRequiredCertifications)  
				.Include(x => x.SopRequiredSkills)
				.FirstOrDefaultAsync(x => x.Id == id, ct);
			return sop;
		}

		public async Task<PaginatedResult<Sop>> GetsPaging(GetsSopQueryFilter query, PaginationRequest request, CancellationToken ct = default)
		{
			var sops = _context.Sops.AsQueryable();

			if (!string.IsNullOrWhiteSpace(query.Name))
			{
				sops = sops.Where(x =>
					EF.Functions.ILike(x.Name, $"%{query.Name}%")
				);
			}

			if (query.ServiceType.HasValue)
			{
				sops = sops.Where(x => x.ServiceType == query.ServiceType.Value);
			}

			if (query.EnvironmentTypeId.HasValue)
			{
				sops = sops.Where(x => x.EnvironmentTypeId == query.EnvironmentTypeId.Value);
			}

			if (query.RequiredSkillIds?.Any() == true)
			{
				sops = sops.Where(x =>
					x.SopRequiredSkills.Any(rs => query.RequiredSkillIds.Contains(rs.SkillId))
				);
			}

			if (query.RequiredCertificationIds?.Any() == true)
			{
				sops = sops.Where(x =>
					x.SopRequiredCertifications.Any(rc => query.RequiredCertificationIds.Contains(rc.CertificationId))
				);
			}

			sops = query.IsDescending ? sops.OrderByDescending(x => x.Name) : sops.OrderBy(x => x.Name);

			return await sops.ToPaginatedResultAsync(request, ct);
		}

		public async Task<List<SopStepMetadataDto>> GetSopStepsWithSchemaAsync(
			Guid sopId,
			CancellationToken ct = default)
		{
			return await _context.SopSteps
				.Where(ss => ss.SopId == sopId && !ss.IsDeleted)
				.Include(ss => ss.Step)
				.OrderBy(ss => ss.StepOrder)
				.Select(ss => new SopStepMetadataDto
				{
					Id = ss.Id,
					SopId = ss.SopId,
					StepId = ss.StepId,
					StepOrder = ss.StepOrder,
					ConfigDetail = ss.ConfigDetail,
					ConfigSchema = ss.Step.ConfigSchema, 
					IsDeleted = ss.IsDeleted
				})
				.ToListAsync(ct);
		}

		public async Task<Sop?> GetSopWithStepDetail(Guid id, CancellationToken ct = default)
		{
			return  await _context.Sops
				.Include(x => x.SopSteps).ThenInclude(ss => ss.Step)
				.Include(x => x.SopRequiredSkills)
				.Include(x => x.SopRequiredCertifications)
				.FirstOrDefaultAsync(x => x.Id == id, ct);
		}
	}
}
