using System.ComponentModel.DataAnnotations.Schema; 

namespace CleanOpsAi.BuildingBlocks.Domain
{
	public abstract class BaseEntity
	{ 
		public Guid Id { get; set; }
	}
}
