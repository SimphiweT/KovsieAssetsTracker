namespace KovsieAssetsTracker.Application.Services
{
    /// <summary>
    /// Defines an abstraction for sending emails via SMTP.
    /// </summary>
    public interface IEmailSender
    {
        /// <summary>
        /// Sends an HTML email asynchronously.
        /// </summary>
        /// <param name="toEmail">Recipient email address.</param>
        /// <param name="subject">Subject line of the email.</param>
        /// <param name="message">HTML body of the email.</param>
        Task SendEmailAsync(string toEmail, string subject, string message);
    }
}
