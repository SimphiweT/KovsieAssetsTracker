using KovsieAssetsTracker.Application.Services;
using KovsieAssetsTracker.Domain;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace KovsieAssetsTracker.Infrastructure.Services
{
    /// <summary>
    /// Handles SMTP email delivery using provided configuration.
    /// </summary>
    public class EmailSender : IEmailSender
    {
        private readonly EmailSettings settings;

        public EmailSender(IOptions<EmailSettings> options)
        {
            settings = options.Value;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            using var client = new SmtpClient(settings.SmtpServer, settings.Port)
            {
                Credentials = new NetworkCredential(settings.Username, settings.Password),
                EnableSsl = true
            };

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(settings.SenderEmail, settings.SenderName),
                Subject = subject,
                Body = message,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            try
            {
                await client.SendMailAsync(mailMessage);
                Console.WriteLine($"Email sent successfully to {toEmail}");
            }
            catch (SmtpException ex)
            {
                Console.WriteLine($"Email sending failed: {ex.Message}");
                throw new Exception("Failed to send email. Please check SMTP configuration.", ex);
            }
        }
    }
}
