namespace CleanOpsAi.Modules.Workforce.Application.Dtos.Maps
{
	public class GoongPlaceDetailResponse
	{
		public GoongPlaceResult? Result { get; set; }
		public string? Status { get; set; }
	}

	public class GoongPlaceResult
	{
		public GoongGeometry? Geometry { get; set; }
		public string? Name { get; set; }
		public string? FormattedAddress { get; set; }
	}
}
