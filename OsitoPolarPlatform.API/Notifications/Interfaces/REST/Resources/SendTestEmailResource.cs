namespace OsitoPolarPlatform.API.Notifications.Interfaces.REST.Resources;

/// <summary>
/// Resource for sending test emails
/// </summary>
public record SendTestEmailResource
{
    public string ToEmail { get; init; } = string.Empty;
    public string ToName { get; init; } = string.Empty;
    public string TemplateName { get; init; } = "CredentialDelivery";
}
