namespace ITStockM.Services.Interfaces
{
    /// <summary>
    /// Interface for email service operations
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Sends an email asynchronously
        /// </summary>
        Task SendEmailAsync(string toEmail, string subject, string body);

        /// <summary>
        /// Sends an email to multiple recipients
        /// </summary>
        Task SendEmailToMultipleAsync(List<string> toEmails, string subject, string body);

        /// <summary>
        /// Sends notification to admin about an action
        /// </summary>
        Task SendAdminNotificationAsync(string action, string details, string performedBy);
    }
}
