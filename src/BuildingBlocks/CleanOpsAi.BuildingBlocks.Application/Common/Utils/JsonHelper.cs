using System.Text.Json;

namespace CleanOpsAi.BuildingBlocks.Application.Common.Utils
{
	public static class JsonHelper
	{
		public static JsonElement ToJsonElement(string json)
		{
			return JsonDocument.Parse(json).RootElement;
		}
	}
}
