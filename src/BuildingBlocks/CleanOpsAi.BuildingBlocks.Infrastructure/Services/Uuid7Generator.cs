using CleanOpsAi.BuildingBlocks.Application.Interfaces;
using Medo;

namespace CleanOpsAi.BuildingBlocks.Infrastructure.Services
{
	public class Uuid7Generator : IIdGenerator
	{
		public Guid Generate() => Uuid7.NewGuid();
	}
}
