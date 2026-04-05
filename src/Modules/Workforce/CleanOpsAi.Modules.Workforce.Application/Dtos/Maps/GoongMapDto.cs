namespace CleanOpsAi.Modules.Workforce.Application.Dtos.Maps
{
	public class GoongMapDto
	{
		public List<GoongResult>? Results { get; set; }
	}

	public class GoongResult
	{
		public GoongGeometry? Geometry { get; set; }
	}

	public class GoongGeometry
	{
		public GoongLocation? Location { get; set; }
	}

	public class GoongLocation
	{
		public double Lat { get; set; }
		public double Lng { get; set; }
	}
}
