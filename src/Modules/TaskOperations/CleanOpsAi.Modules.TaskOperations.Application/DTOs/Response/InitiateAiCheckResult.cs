using CleanOpsAi.Modules.TaskOperations.Domain.Enums;

namespace CleanOpsAi.Modules.TaskOperations.Application.DTOs.Response
{ 
    public sealed class InitiateAiCheckResult
    { 
        public Guid ComplianceCheckId { get; init; }  
        
        public ComplianceCheckType Type { get; init; } 

        public ComplianceCheckStatus Status { get; init; }

		public DateTime Created { get; init; }
    }
}
