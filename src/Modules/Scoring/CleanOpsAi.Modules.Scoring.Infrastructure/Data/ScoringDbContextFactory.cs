using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CleanOpsAi.Modules.Scoring.Infrastructure.Data
{
	public class ScoringDbContextFactory : IDesignTimeDbContextFactory<ScoringDbContext>
	{
		public ScoringDbContext CreateDbContext(string[] args)
		{
			var optionsBuilder = new DbContextOptionsBuilder<ScoringDbContext>();

			var connectionString =
				Environment.GetEnvironmentVariable("SCORING_DB_CONNECTION")
				?? Environment.GetEnvironmentVariable("DEFAULT_CONNECTION_STRING")
				?? "Host=localhost;Port=5432;Database=cleanopsai;Username=postgres;Password=postgres";

			optionsBuilder
				.UseNpgsql(connectionString)
				.UseSnakeCaseNamingConvention();

			return new ScoringDbContext(optionsBuilder.Options);
		}
	}
}
