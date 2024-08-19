using Application.Common.Abstractions;
using System;
using System.Threading.Tasks;

namespace Api.Authentication.Utilities
{
    public static class VerificationCode
    {
        public static string GenerateVerificationCode()
        {
            Random random = new Random();
            int code = random.Next(100000, 999999);
            return code.ToString();
        }

        public static async Task SendVerificationCodeAsync(IEmailService emailService, string email, string code)
        {
            await emailService.SendEmailAsync(email, "Animal Tracking Password Reset Verification Code", $"Your verification code is {code}");
        }
    }
}
