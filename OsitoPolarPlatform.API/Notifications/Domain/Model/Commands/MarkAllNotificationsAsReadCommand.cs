namespace OsitoPolarPlatform.API.Notifications.Domain.Model.Commands;

/// <summary>
/// Command to mark all notifications as read for a user
/// </summary>
public record MarkAllNotificationsAsReadCommand(int UserId);
