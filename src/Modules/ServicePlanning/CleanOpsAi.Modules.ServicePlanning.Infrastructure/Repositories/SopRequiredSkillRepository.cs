using CleanOpsAi.Modules.ServicePlanning.Application.Common.Interfaces.Repositories;
using CleanOpsAi.Modules.ServicePlanning.Domain.Entities;
using CleanOpsAi.Modules.ServicePlanning.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CleanOpsAi.Modules.ServicePlanning.Infrastructure.Repositories
{
	public class SopRequiredSkillRepository : ISopRequiredSkillRepository
	{
		private readonly ServicePlanningDbContext _context;

		public SopRequiredSkillRepository(ServicePlanningDbContext context)
		{
			_context = context;
		} 

		public async Task MergeAsync(Guid sopId, HashSet<Guid> skillIds)
		{
			await _context.SopRequiredSkills
				.Where(x => x.SopId == sopId && !skillIds.Contains(x.SkillId))
				.ExecuteDeleteAsync();

			var existingIds = (await _context.SopRequiredSkills
				.Where(x => x.SopId == sopId)
				.Select(x => x.SkillId)
				.ToListAsync())
				.ToHashSet();

			var toAdd = skillIds.Except(existingIds)
				.Select(id => new SopRequiredSkill { SopId = sopId, SkillId = id });

			await _context.SopRequiredSkills.AddRangeAsync(toAdd);
		}
	}
}
