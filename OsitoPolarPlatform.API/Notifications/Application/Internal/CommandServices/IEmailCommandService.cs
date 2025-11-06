using OsitoPolarPlatform.API.Notifications.Domain.Model.Commands;

namespace OsitoPolarPlatform.API.Notifications.Application.Internal.CommandServices;

/// <summary>
/// Email command service interface
/// </summary>
public interface IEmailCommandService
{
    /// <summary>
    /// Send an email using a template
    /// </summary>
    Task<bool> SendTemplatedEmailAsync(SendEmailCommand command);

    /// <summary>
    /// Send a raw HTML email
    /// </summary>
    Task<bool> SendRawEmailAsync(string to, string toName, string subject, string htmlBody);
}
