public interface IEmailService
{
    Task SendAsync(string to, string subject, string body, bool isHtml = true);
    Task SendPasswordResetAsync(string to, string resetUrl);
    Task SendEmailConfirmationAsync(string to, string confirmUrl);
}
