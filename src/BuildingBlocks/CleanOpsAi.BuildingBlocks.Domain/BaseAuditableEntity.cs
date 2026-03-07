namespace CleanOpsAi.BuildingBlocks.Domain
{
	public abstract class BaseAuditableEntity : BaseEntity
	{
		public bool IsDeleted { get; set; }

		public DateTime Created { get; set; }

		public string? CreatedBy { get; set; }

		public DateTime LastModified { get; set; }

		public string? LastModifiedBy { get; set; }
	}
}
