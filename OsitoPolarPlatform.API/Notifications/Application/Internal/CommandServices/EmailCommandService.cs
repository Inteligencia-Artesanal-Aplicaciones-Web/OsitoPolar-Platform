using OsitoPolarPlatform.API.Notifications.Domain.Model.Commands;
using OsitoPolarPlatform.API.Notifications.Domain.Model.Entities;
using OsitoPolarPlatform.API.Notifications.Domain.Model.ValueObjects;
using OsitoPolarPlatform.API.Notifications.Domain.Repositories;
using OsitoPolarPlatform.API.Notifications.Domain.Services;
using OsitoPolarPlatform.API.Shared.Domain.Repositories;

namespace OsitoPolarPlatform.API.Notifications.Application.Internal.CommandServices;

/// <summary>
/// Email command service implementation
/// Orchestrates template rendering, email sending, and logging
/// </summary>
public class EmailCommandService : IEmailCommandService
{
    private readonly IEmailProvider _emailProvider;
    private readonly ITemplateRenderer _templateRenderer;
    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<EmailCommandService> _logger;

    public EmailCommandService(
        IEmailProvider emailProvider,
        ITemplateRenderer templateRenderer,
        INotificationRepository notificationRepository,
        IUnitOfWork unitOfWork,
        ILogger<EmailCommandService> logger)
    {
        _emailProvider = emailProvider;
        _templateRenderer = templateRenderer;
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> SendTemplatedEmailAsync(SendEmailCommand command)
    {
        var notificationLog = new NotificationLog
        {
            Type = NotificationType.Email,
            Recipient = command.To,
            Template = command.Template,
            Status = NotificationStatus.Pending
        };

        try
        {
            _logger.LogInformation(
                "Sending templated email to {To} using template {Template}",
                command.To,
                command.Template
            );

            // Render template with data
            var htmlBody = await _templateRenderer.RenderTemplateAsync(
                command.Template,
                command.TemplateData
            );

            // Get subject based on template
            var subject = GetSubjectForTemplate(command.Template);

            // Send email
            var emailRequest = new EmailRequest
            {
                To = command.To,
                ToName = command.ToName,
                Subject = subject,
                HtmlBody = htmlBody
            };

            var result = await _emailProvider.SendEmailAsync(emailRequest);

            if (result.Success)
            {
                notificationLog.MarkAsSent();
                _logger.LogInformation(
                    "Email sent successfully to {To} using template {Template}",
                    command.To,
                    command.Template
                );
            }
            else
            {
                notificationLog.MarkAsFailed(result.ErrorMessage ?? "Unknown error");
                _logger.LogError(
                    "Failed to send email to {To}: {Error}",
                    command.To,
                    result.ErrorMessage
                );
            }

            // Save notification log
            await _notificationRepository.AddAsync(notificationLog);
            await _unitOfWork.CompleteAsync();

            return result.Success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending templated email to {To}", command.To);
            notificationLog.MarkAsFailed(ex.Message);
            await _notificationRepository.AddAsync(notificationLog);
            await _unitOfWork.CompleteAsync();
            return false;
        }
    }

    public async Task<bool> SendRawEmailAsync(string to, string toName, string subject, string htmlBody)
    {
        var notificationLog = new NotificationLog
        {
            Type = NotificationType.Email,
            Recipient = to,
            Template = null,
            Status = NotificationStatus.Pending
        };

        try
        {
            _logger.LogInformation("Sending raw email to {To} with subject: {Subject}", to, subject);

            var emailRequest = new EmailRequest
            {
                To = to,
                ToName = toName,
                Subject = subject,
                HtmlBody = htmlBody
            };

            var result = await _emailProvider.SendEmailAsync(emailRequest);

            if (result.Success)
            {
                notificationLog.MarkAsSent();
                _logger.LogInformation("Raw email sent successfully to {To}", to);
            }
            else
            {
                notificationLog.MarkAsFailed(result.ErrorMessage ?? "Unknown error");
                _logger.LogError("Failed to send raw email to {To}: {Error}", to, result.ErrorMessage);
            }

            await _notificationRepository.AddAsync(notificationLog);
            await _unitOfWork.CompleteAsync();

            return result.Success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending raw email to {To}", to);
            notificationLog.MarkAsFailed(ex.Message);
            await _notificationRepository.AddAsync(notificationLog);
            await _unitOfWork.CompleteAsync();
            return false;
        }
    }

    private static string GetSubjectForTemplate(EmailTemplate template)
    {
        return template switch
        {
            EmailTemplate.Welcome => "Welcome to OsitoPolar! 🐻‍❄️",
            EmailTemplate.PasswordReset => "Password Reset Request - OsitoPolar",
            EmailTemplate.CredentialDelivery => "Your OsitoPolar Login Credentials",
            EmailTemplate.TransactionConfirmation => "Payment Confirmation - OsitoPolar",
            EmailTemplate.TwoFactorSetup => "Complete Your Two-Factor Authentication Setup",
            _ => "OsitoPolar Notification"
        };
    }
}
