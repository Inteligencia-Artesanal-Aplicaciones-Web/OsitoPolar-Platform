namespace OsitoPolarPlatform.API.Notifications.Domain.Services;

/// <summary>
/// Email provider interface for dependency injection
/// Allows multiple email providers (MailerSend, SendGrid, AWS SES, etc.)
/// </summary>
public interface IEmailProvider
{
    /// <summary>
    /// Name of the provider (e.g., "MailerSend", "SendGrid", "AWS SES")
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Send an email asynchronously
    /// </summary>
    Task<EmailResult> SendEmailAsync(EmailRequest request);
}

/// <summary>
/// Email request containing all necessary information to send an email
/// </summary>
public record EmailRequest
{
    public string To { get; init; } = string.Empty;
    public string? ToName { get; init; }
    public string Subject { get; init; } = string.Empty;
    public string HtmlBody { get; init; } = string.Empty;
    public string? PlainTextBody { get; init; }
    public Dictionary<string, string>? Metadata { get; init; }
}

/// <summary>
/// Result of an email sending operation
/// </summary>
public record EmailResult
{
    public bool Success { get; init; }
    public string? MessageId { get; init; }
    public string? ErrorMessage { get; init; }

    public EmailResult(bool success, string? messageId = null, string? errorMessage = null)
    {
        Success = success;
        MessageId = messageId;
        ErrorMessage = errorMessage;
    }

    public static EmailResult SuccessResult(string? messageId = null) =>
        new(true, messageId);

    public static EmailResult FailureResult(string errorMessage) =>
        new(false, null, errorMessage);
}
