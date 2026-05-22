namespace CleanOpsAi.BuildingBlocks.Infrastructure.Events.Request
{
    public class GetAssignedWorkAreasRequest
    {
        public List<Guid> AssignedWorkAreaIds { get; set; } = new();
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetUnassignedWorkAreasRequest
    {
        public List<Guid> AssignedWorkAreaIds { get; set; } = new();
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetWorkAreasByAssignmentStatusResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalElements { get; set; }
        public int TotalPages { get; set; }
        public List<WorkAreaWithLocationDto> Items { get; set; } = new();
    }
}
