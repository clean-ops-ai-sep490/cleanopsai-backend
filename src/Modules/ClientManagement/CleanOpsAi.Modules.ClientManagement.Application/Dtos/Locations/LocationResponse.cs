namespace CleanOpsAi.Modules.ClientManagement.Application.Dtos.Locations
{
    public class LocationResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Address { get; set; } = null!;
		public string? Street { get; set; }
        public string? Commune { get; set; }
        public string? Province { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public Guid ClientId { get; set; }
        public string ClientName { get; set; } = null!;
    }
}
