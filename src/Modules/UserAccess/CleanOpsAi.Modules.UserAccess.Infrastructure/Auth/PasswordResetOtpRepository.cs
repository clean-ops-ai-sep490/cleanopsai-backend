using CleanOpsAi.Modules.UserAccess.Application.Contracts;
using CleanOpsAi.Modules.UserAccess.Domain;
using CleanOpsAi.Modules.UserAccess.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.UserAccess.Infrastructure.Auth
{
    public class PasswordResetOtpRepository : IPasswordResetOtpRepository
    {
        private readonly UserAccessDbContext _dbContext;

        public PasswordResetOtpRepository(UserAccessDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(PasswordResetOtp otp)
        {
            await _dbContext.Set<PasswordResetOtp>().AddAsync(otp);
        }

        public async Task<PasswordResetOtp?> GetValidOtpAsync(string email, string otp)
        {
            return await _dbContext.Set<PasswordResetOtp>()
                .FirstOrDefaultAsync(x =>
                    x.Email == email &&
                    x.OtpCode == otp &&
                    x.ExpiredAt > DateTime.UtcNow &&
                    !x.IsUsed);
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
