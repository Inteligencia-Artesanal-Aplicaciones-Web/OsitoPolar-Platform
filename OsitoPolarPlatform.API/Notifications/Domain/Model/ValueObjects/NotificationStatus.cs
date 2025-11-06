namespace OsitoPolarPlatform.API.Notifications.Domain.Model.ValueObjects;

/// <summary>
/// Status of a notification
/// </summary>
public enum NotificationStatus
{
    /// <summary>
    /// Notification is pending to be sent
    /// </summary>
    Pending,

    /// <summary>
    /// Notification was sent successfully
    /// </summary>
    Sent,

    /// <summary>
    /// Notification failed to send
    /// </summary>
    Failed
}
