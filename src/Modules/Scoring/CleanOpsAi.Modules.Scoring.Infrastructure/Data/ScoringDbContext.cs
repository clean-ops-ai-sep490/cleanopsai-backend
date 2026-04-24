using CleanOpsAi.Modules.Scoring.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace CleanOpsAi.Modules.Scoring.Infrastructure.Data
{
	public class ScoringDbContext : DbContext
	{
		public DbSet<ScoringJob> ScoringJobs { get; set; }
		public DbSet<ScoringJobResult> ScoringJobResults { get; set; }
		public DbSet<ScoringAnnotationCandidate> ScoringAnnotationCandidates { get; set; }
		public DbSet<ScoringAnnotation> ScoringAnnotations { get; set; }
		public DbSet<ScoringRetrainBatch> ScoringRetrainBatches { get; set; }
		public DbSet<ScoringRetrainRun> ScoringRetrainRuns { get; set; }

		public ScoringDbContext()
		{
		}

		public ScoringDbContext(DbContextOptions<ScoringDbContext> options)
			: base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.HasDefaultSchema("scoring");
			base.OnModelCreating(modelBuilder);
			modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
		}
	}
}
