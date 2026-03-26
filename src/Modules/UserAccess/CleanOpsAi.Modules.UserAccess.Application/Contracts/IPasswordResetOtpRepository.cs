using CleanOpsAi.Modules.UserAccess.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.UserAccess.Application.Contracts
{
    public interface IPasswordResetOtpRepository
    {
        Task AddAsync(PasswordResetOtp otp);
        Task<PasswordResetOtp?> GetValidOtpAsync(string email, string otp);
        Task SaveChangesAsync();
    }
}
