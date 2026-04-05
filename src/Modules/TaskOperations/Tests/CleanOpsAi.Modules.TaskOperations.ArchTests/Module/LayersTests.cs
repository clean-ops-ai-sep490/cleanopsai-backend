using CleanOpsAi.Modules.TaskOperations.ArchTests.SeedWork;
using NetArchTest.Rules;

namespace CleanOpsAi.Modules.ServicePlanning.ArchTests.Module
{
	public class LayersTests : TestBase
	{
		[Fact]
		public void DomainLayer_DoesNotHaveDependency_ToApplicationLayer()
		{
			var result = Types.InAssembly(DomainAssembly)
				.ShouldNot()
				.HaveDependencyOn(ApplicationAssembly.GetName().Name)
				.GetResult();
			Assert.True(result.IsSuccessful,
				$"Failing Types: {string.Join(", ", result.FailingTypeNames ?? [])}");
		}

		[Fact]
		public void DomainLayer_DoesNotHaveDependency_ToInfrastructureLayer()
		{
			var result = Types.InAssembly(DomainAssembly)
				.ShouldNot()
				.HaveDependencyOn(InfrastructureAssembly.GetName().Name)
				.GetResult();
			Assert.True(result.IsSuccessful,
				$"Failing Types: {string.Join(", ", result.FailingTypeNames ?? [])}");
		}

		[Fact]
		public void ApplicationLayer_DoesNotHaveDependency_ToInfrastructureLayer()
		{
			var result = Types.InAssembly(ApplicationAssembly)
				.ShouldNot()
				.HaveDependencyOn(InfrastructureAssembly.GetName().Name)
				.GetResult();
			Assert.True(result.IsSuccessful,
				$"Failing Types: {string.Join(", ", result.FailingTypeNames ?? [])}");
		}
	}
}
