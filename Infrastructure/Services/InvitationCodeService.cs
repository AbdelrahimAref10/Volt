using System;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public interface IInvitationCodeService
    {
        string GenerateInvitationCode();
        Task SendInvitationCodeAsync(string mobileNumber, string code);
    }

    public class InvitationCodeService : IInvitationCodeService
    {
        private readonly Random _random = new Random();

        public string GenerateInvitationCode()
        {
            // Generate 6-digit code
            return _random.Next(100000, 999999).ToString();
        }

        public async Task SendInvitationCodeAsync(string mobileNumber, string code)
        {
            // TODO: Implement SMS service integration
            // For now, just log it (remove in production)
            Console.WriteLine($"SMS to {mobileNumber}: Your activation code is {code}");
            
            // Simulate async operation
            await Task.CompletedTask;
            
            // In production, integrate with SMS provider like:
            // - Twilio
            // - AWS SNS
            // - Azure Communication Services
            // - Local SMS gateway
        }
    }
}

