namespace OsitoPolarPlatform.API.Notifications.Domain.Model.Aggregates;

/// <summary>
/// Represents an in-app notification for a user
/// Used for alerts about equipment status, service requests, payments, etc.
/// </summary>
public class InAppNotification
{
    public int Id { get; private set; }

    /// <summary>
    /// User ID who will receive this notification
    /// </summary>
    public int UserId { get; private set; }

    /// <summary>
    /// Notification title (e.g., "Equipment Alert", "Service Accepted")
    /// </summary>
    public string Title { get; private set; }

    /// <summary>
    /// Notification message/description
    /// </summary>
    public string Message { get; private set; }

    /// <summary>
    /// Type of notification: "equipment_alert", "service_request", "payment", "system"
    /// </summary>
    public string Type { get; private set; }

    /// <summary>
    /// Severity level: "info", "warning", "error", "success"
    /// </summary>
    public string Severity { get; private set; }

    /// <summary>
    /// Related equipment ID (optional)
    /// </summary>
    public int? EquipmentId { get; private set; }

    /// <summary>
    /// Related service request ID (optional)
    /// </summary>
    public int? ServiceRequestId { get; private set; }

    /// <summary>
    /// Is this notification read?
    /// </summary>
    public bool IsRead { get; private set; }

    /// <summary>
    /// When was this notification created
    /// </summary>
    public DateTime Timestamp { get; private set; }

    /// <summary>
    /// When was this notification read (optional)
    /// </summary>
    public DateTime? ReadAt { get; private set; }

    /// <summary>
    /// Additional metadata (JSON string)
    /// </summary>
    public string? Metadata { get; private set; }

    protected InAppNotification()
    {
        Title = string.Empty;
        Message = string.Empty;
        Type = string.Empty;
        Severity = "info";
        Timestamp = DateTime.UtcNow;
    }

    public InAppNotification(
        int userId,
        string title,
        string message,
        string type,
        string severity = "info",
        int? equipmentId = null,
        int? serviceRequestId = null,
        string? metadata = null)
    {
        UserId = userId;
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Message = message ?? throw new ArgumentNullException(nameof(message));
        Type = type ?? throw new ArgumentNullException(nameof(type));
        Severity = severity ?? "info";
        EquipmentId = equipmentId;
        ServiceRequestId = serviceRequestId;
        IsRead = false;
        Timestamp = DateTime.UtcNow;
        Metadata = metadata;
    }

    /// <summary>
    /// Mark this notification as read
    /// </summary>
    public void MarkAsRead()
    {
        if (!IsRead)
        {
            IsRead = true;
            ReadAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Mark this notification as unread
    /// </summary>
    public void MarkAsUnread()
    {
        IsRead = false;
        ReadAt = null;
    }
}
