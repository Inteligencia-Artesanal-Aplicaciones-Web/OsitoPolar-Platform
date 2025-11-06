using OsitoPolarPlatform.API.Notifications.Domain.Model.ValueObjects;

namespace OsitoPolarPlatform.API.Notifications.Domain.Model.Entities;

/// <summary>
/// Entity to log all notification attempts
/// </summary>
public class NotificationLog
{
    public int Id { get; set; }
    public NotificationType Type { get; set; }
    public string Recipient { get; set; } = string.Empty;
    public EmailTemplate? Template { get; set; }
    public NotificationStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Additional metadata for the notification (JSON)
    /// </summary>
    public string? Metadata { get; set; }

    public NotificationLog() { }

    public NotificationLog(NotificationType type, string recipient, EmailTemplate? template, NotificationStatus status)
    {
        Type = type;
        Recipient = recipient;
        Template = template;
        Status = status;
    }

    public void MarkAsSent()
    {
        Status = NotificationStatus.Sent;
        SentAt = DateTime.UtcNow;
    }

    public void MarkAsFailed(string errorMessage)
    {
        Status = NotificationStatus.Failed;
        ErrorMessage = errorMessage;
        SentAt = DateTime.UtcNow;
    }
}
