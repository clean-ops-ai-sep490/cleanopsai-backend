namespace CleanOpsAi.Modules.ClientManagement.Application.Dtos.WorkareaDetails
{
    public class WorkAreaDetailResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public double Area { get; set; }
        public double TotalArea { get; set; }
        public Guid WorkAreaId { get; set; }
        public string? WorkAreaName { get; set; }
    }
}
