using Anir.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;

    public EmailService(IOptions<EmailSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task SendAsync(string to, string subject, string body, bool isHtml = true)
    {
        var message = new MailMessage
        {
            From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
            Subject = subject,
            Body = body,
            IsBodyHtml = isHtml
        };

        message.To.Add(to);

        using var client = new SmtpClient(_settings.SmtpServer, _settings.SmtpPort)
        {
            Credentials = new NetworkCredential(_settings.SenderEmail, _settings.SenderPassword),
            EnableSsl = true
        };

        await client.SendMailAsync(message);
    }

    public async Task SendPasswordResetAsync(string to, string resetUrl)
    {
        var templatePath = Path.Combine(AppContext.BaseDirectory, "Templates", "ResetPassword.html");

        var html = await File.ReadAllTextAsync(templatePath);

        // Reemplazar el placeholder {{LINK}}
        html = html.Replace("{{LINK}}", resetUrl);

        await SendAsync(to, "Restablecer contraseña", html, isHtml: true);
    }

    public async Task SendEmailConfirmationAsync(string to, string confirmUrl)
    {
        var templatePath = Path.Combine(AppContext.BaseDirectory, "Email", "Templates", "ConfirmEmail.html");

        var html = await File.ReadAllTextAsync(templatePath);

        // Reemplazar el placeholder {{LINK}}
        html = html.Replace("{{LINK}}", confirmUrl);

        await SendAsync(to, "Confirmar cuenta", html, isHtml: true);
    }

}
