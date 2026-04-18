using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.UserAccess.Domain
{
    public class PasswordResetOtp
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string OtpCode { get; set; }
        public DateTime ExpiredAt { get; set; }
        public bool IsUsed { get; set; }
    }
}
