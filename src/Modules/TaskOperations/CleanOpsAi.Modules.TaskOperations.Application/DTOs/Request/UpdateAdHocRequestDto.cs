using CleanOpsAi.Modules.TaskOperations.Application.Converters;
using CleanOpsAi.Modules.TaskOperations.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.TaskOperations.Application.DTOs.Request
{
    public class UpdateAdHocRequestDto
    {
        public Guid? WorkAreaId { get; set; }

        [JsonConverter(typeof(NullableEnumConverter<AdHocRequestType>))]
        public AdHocRequestType? RequestType { get; set; }

        [JsonConverter(typeof(NullableDateTimeConverter))]
        public DateTime? RequestDateFrom { get; set; }

        [JsonConverter(typeof(NullableDateTimeConverter))]
        public DateTime? RequestDateTo { get; set; }

        public string? Reason { get; set; }

        public string? Description { get; set; }
    }
}
