namespace OsitoPolarPlatform.API.Notifications.Domain.Model.Commands;

/// <summary>
/// Command to mark a notification as read
/// </summary>
public record MarkNotificationAsReadCommand(int NotificationId, int UserId);
