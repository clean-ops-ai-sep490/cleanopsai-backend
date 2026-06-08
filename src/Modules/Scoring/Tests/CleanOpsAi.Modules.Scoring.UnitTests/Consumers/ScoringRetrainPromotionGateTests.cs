using CleanOpsAi.Modules.Scoring.Infrastructure.Consumers;
using CleanOpsAi.Modules.Scoring.Infrastructure.Options;
using System.Reflection;
using System.Text.Json.Nodes;

namespace CleanOpsAi.Modules.Scoring.UnitTests.Consumers
{
	public class ScoringRetrainPromotionGateTests
	{
		[Fact]
		public void BenchmarkGate_ShouldPromote_WhenCandidateMeetsRequiredBenchmarkMiou()
		{
			var result = EvaluateBenchmarkGateFromMetrics(
				"""
				{
				  "benchmark": {
				    "candidate": { "mean_iou": 0.4300 },
				    "baseline": { "mean_iou": 0.4186 }
				  },
				  "unet": { "miou": 0.1000 }
				}
				""");

			Assert.True(Get<bool>(result, "Promoted"));
			Assert.Equal("benchmark.unet_mean_iou", Get<string>(result, "MetricKey"));
			Assert.Equal(0.4300, Get<double>(result, "CandidateCompositeMetric"), 4);
			Assert.Equal(0.4186, Get<double>(result, "BaselineCompositeMetric"), 4);
			Assert.Equal(0.005, Get<double>(result, "MinimumImprovement"), 4);
		}

		[Fact]
		public void BenchmarkGate_ShouldReject_WhenCandidateMissesRequiredBenchmarkMiou()
		{
			var result = EvaluateBenchmarkGateFromMetrics(
				"""
				{
				  "benchmark": {
				    "candidate": { "mean_iou": 0.4200 },
				    "baseline": { "mean_iou": 0.4186 }
				  }
				}
				""");

			Assert.False(Get<bool>(result, "Promoted"));
			Assert.Contains("Rejected: benchmark mIoU", Get<string>(result, "Reason"));
			Assert.Equal("benchmark.unet_mean_iou", Get<string>(result, "MetricKey"));
		}

		[Fact]
		public void BenchmarkGate_ShouldReject_WhenBenchmarkMetricMissing_EvenIfTrainingMetricExists()
		{
			var result = EvaluateBenchmarkGateFromMetrics(
				"""
				{
				  "unet": { "miou": 0.9990 },
				  "yolo": { "map": 0.9990 }
				}
				""");

			Assert.False(Get<bool>(result, "Promoted"));
			Assert.Equal(0, Get<double>(result, "CandidateCompositeMetric"));
			Assert.Equal("benchmark.unet_mean_iou", Get<string>(result, "MetricKey"));
			Assert.Contains("Benchmark metrics missing key", Get<string>(result, "Reason"));
		}

		private static object EvaluateBenchmarkGateFromMetrics(string json)
		{
			var method = typeof(ScoringRetrainRequestedConsumer).GetMethod(
				"EvaluateBenchmarkGateFromMetrics",
				BindingFlags.NonPublic | BindingFlags.Static);

			Assert.NotNull(method);

			var options = new ScoringRetrainOptions
			{
				UseBenchmarkPromotionGate = true,
				MinimumBenchmarkMiouImprovement = 0.005,
			};
			var node = JsonNode.Parse(json);
			var result = method!.Invoke(null, new object?[] { node, options });

			Assert.NotNull(result);
			return result!;
		}

		private static T Get<T>(object source, string propertyName)
		{
			var property = source.GetType().GetProperty(
				propertyName,
				BindingFlags.Public | BindingFlags.Instance);

			Assert.NotNull(property);
			return Assert.IsType<T>(property!.GetValue(source));
		}
	}
}
