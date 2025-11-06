using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using OsitoPolarPlatform.API.Notifications.Domain.Services;

namespace OsitoPolarPlatform.API.Notifications.Infrastructure.External.MailerSend;

/// <summary>
/// MailerSend email provider implementation using SMTP
/// </summary>
public class MailerSendEmailProvider : IEmailProvider
{
    private readonly MailerSendConfiguration _config;
    private readonly ILogger<MailerSendEmailProvider> _logger;

    public string ProviderName => "MailerSend";

    public MailerSendEmailProvider(
        IOptions<MailerSendConfiguration> config,
        ILogger<MailerSendEmailProvider> logger)
    {
        _config = config.Value;
        _logger = logger;
    }

    public async Task<EmailResult> SendEmailAsync(EmailRequest request)
    {
        try
        {
            _logger.LogInformation(
                "Sending email via MailerSend SMTP to {To} with subject: {Subject}",
                request.To,
                request.Subject
            );

            // Create mail message
            using var mailMessage = new MailMessage
            {
                From = new MailAddress(_config.FromEmail, _config.FromName),
                Subject = request.Subject,
                Body = request.HtmlBody,
                IsBodyHtml = true
            };

            // Add recipient
            mailMessage.To.Add(new MailAddress(request.To, request.ToName ?? request.To));

            // Add plain text alternative if provided
            if (!string.IsNullOrEmpty(request.PlainTextBody))
            {
                var plainTextView = AlternateView.CreateAlternateViewFromString(
                    request.PlainTextBody,
                    null,
                    "text/plain"
                );
                mailMessage.AlternateViews.Add(plainTextView);
            }

            // Create SMTP client
            using var smtpClient = new SmtpClient(_config.SmtpHost, _config.SmtpPort)
            {
                Credentials = new NetworkCredential(_config.SmtpUsername, _config.SmtpPassword),
                EnableSsl = _config.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Timeout = 30000 // 30 seconds
            };

            // Send email
            await smtpClient.SendMailAsync(mailMessage);

            _logger.LogInformation(
                "Email sent successfully via MailerSend to {To}",
                request.To
            );

            return EmailResult.SuccessResult(Guid.NewGuid().ToString());
        }
        catch (SmtpException ex)
        {
            _logger.LogError(
                ex,
                "SMTP error sending email to {To}: {Message}",
                request.To,
                ex.Message
            );
            return EmailResult.FailureResult($"SMTP Error: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error sending email to {To}",
                request.To
            );
            return EmailResult.FailureResult($"Error: {ex.Message}");
        }
    }
}
