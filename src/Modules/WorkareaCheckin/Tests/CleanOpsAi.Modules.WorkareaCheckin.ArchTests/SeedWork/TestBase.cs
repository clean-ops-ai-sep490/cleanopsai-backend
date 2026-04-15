using CleanOpsAi.Modules.WorkareaCheckin.Application;
using CleanOpsAi.Modules.WorkareaCheckin.Domain;
using CleanOpsAi.Modules.WorkareaCheckin.Infrastructure;
using System.Reflection;
using NetArchTest.Rules;

namespace CleanOpsAi.Modules.WorkareaCheckin.ArchTests.SeedWork
{
	public abstract class TestBase
	{
		protected static Assembly DomainAssembly => typeof(DomainAssemblyMarker).Assembly;

		protected static Assembly ApplicationAssembly => typeof(ApplicationAssemblyMarker).Assembly;

		protected static Assembly InfrastructureAssembly => typeof(InfrastructureAssemblyMarker).Assembly;

		protected static void AssertAreImmutable(IEnumerable<Type> types)
		{
			var failingTypes = new List<Type>();

			foreach (var type in types)
			{
				if (type.GetFields().Any(x => !x.IsInitOnly) ||
					type.GetProperties().Any(x => x.CanWrite))
				{
					failingTypes.Add(type);
				}
			}

			AssertFailingTypes(failingTypes);
		}

		protected static void AssertFailingTypes(IEnumerable<Type> types)
		{
			Assert.True(!types.Any(),
				$"Failing Types: {string.Join(", ", types.Select(t => t.Name))}");
		}

		protected static void AssertArchTestResult(TestResult result)
		{
			Assert.True(result.IsSuccessful,
				$"Failing Types: {string.Join(", ", result.FailingTypeNames ?? [])}");
		}
	}
}