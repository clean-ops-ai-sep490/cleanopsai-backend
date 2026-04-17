using CleanOpsAi.BuildingBlocks.Domain.Dtos.Notifications; 

namespace CleanOpsAi.Modules.QualityControl.Application.Common.Mappings
{
	public static class RecipientTypeMapper
	{
		private static readonly Dictionary<string, RecipientTypeEnum> _roleMap = new()
		{
			["Admin"] = RecipientTypeEnum.Admin,
			["Manager"] = RecipientTypeEnum.Manager,
			["Supervisor"] = RecipientTypeEnum.Supervisor,
			["Supporter"] = RecipientTypeEnum.Supporter,
			["Worker"] = RecipientTypeEnum.Worker,
		};

		public static RecipientTypeEnum FromRole(string role) =>
			_roleMap.TryGetValue(role, out var type)
				? type
				: throw new InvalidOperationException($"Unknown role: {role}");

		public static bool IsRoleBased(RecipientTypeEnum type) =>
			type is RecipientTypeEnum.Admin
				or RecipientTypeEnum.Manager
				or RecipientTypeEnum.Supporter;
	}
}
