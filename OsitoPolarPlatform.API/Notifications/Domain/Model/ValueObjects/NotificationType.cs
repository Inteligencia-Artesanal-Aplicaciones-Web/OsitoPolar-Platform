namespace OsitoPolarPlatform.API.Notifications.Domain.Model.ValueObjects;

/// <summary>
/// Type of notification to be sent
/// </summary>
public enum NotificationType
{
    /// <summary>
    /// Email notification
    /// </summary>
    Email,

    /// <summary>
    /// SMS notification (future)
    /// </summary>
    Sms,

    /// <summary>
    /// Push notification (future)
    /// </summary>
    Push
}
