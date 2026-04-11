namespace CleanOpsAi.Modules.WorkareaCheckin.Domain.Enums
{
	public enum CheckinStatus
	{
		Active,    // đã checkin, đang trong ca
		Invalid,   // checkin lỗi, không hợp lệ
		Cancelled  // bị hủy
	}
}
