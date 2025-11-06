using OsitoPolarPlatform.API.Notifications.Domain.Model.ValueObjects;

namespace OsitoPolarPlatform.API.Notifications.Domain.Model.Commands;

/// <summary>
/// Command to send an email using a template
/// </summary>
public record SendEmailCommand
{
    public string To { get; init; } = string.Empty;
    public string? ToName { get; init; }
    public EmailTemplate Template { get; init; }
    public Dictionary<string, object> TemplateData { get; init; } = new();
}
