using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly string _fromEmail;
        private readonly string _fromName;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
            _smtpServer = _configuration["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
            _smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
            _smtpUsername = _configuration["EmailSettings:Username"] ?? "";
            _smtpPassword = _configuration["EmailSettings:Password"] ?? "";
            _fromEmail = _configuration["EmailSettings:FromEmail"] ?? "noreply@volt.com";
            _fromName = _configuration["EmailSettings:FromName"] ?? "Volt System";
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true)
        {
            try
            {
                // For development/testing: If no SMTP credentials, just log
                if (string.IsNullOrWhiteSpace(_smtpUsername) || string.IsNullOrWhiteSpace(_smtpPassword))
                {
                    Console.WriteLine($"EMAIL (Mock) to {toEmail}:");
                    Console.WriteLine($"Subject: {subject}");
                    Console.WriteLine($"Body: {body}");
                    await Task.CompletedTask;
                    return;
                }

                using (var client = new SmtpClient(_smtpServer, _smtpPort))
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);

                    using (var message = new MailMessage())
                    {
                        message.From = new MailAddress(_fromEmail, _fromName);
                        message.To.Add(new MailAddress(toEmail));
                        message.Subject = subject;
                        message.Body = body;
                        message.IsBodyHtml = isHtml;

                        await client.SendMailAsync(message);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but don't throw - we don't want registration to fail if email fails
                Console.WriteLine($"Error sending email to {toEmail}: {ex.Message}");
                // In production, use proper logging service
            }
        }

        public async Task SendActivationCodeAsync(string toEmail, string activationCode)
        {
            var subject = "Your Activation Code - Volt";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif; padding: 20px;'>
                    <h2 style='color: #4F46E5;'>Welcome to Volt!</h2>
                    <p>Thank you for registering with Volt. Please use the following activation code to activate your account:</p>
                    <div style='background-color: #F3F4F6; padding: 15px; border-radius: 5px; text-align: center; margin: 20px 0;'>
                        <h1 style='color: #4F46E5; margin: 0; letter-spacing: 5px;'>{activationCode}</h1>
                    </div>
                    <p>This code will expire in 24 hours.</p>
                    <p>If you did not request this code, please ignore this email.</p>
                    <hr style='border: none; border-top: 1px solid #E5E7EB; margin: 20px 0;'/>
                    <p style='color: #6B7280; font-size: 12px;'>This is an automated message, please do not reply.</p>
                </body>
                </html>";

            await SendEmailAsync(toEmail, subject, body);
        }
    }
}


