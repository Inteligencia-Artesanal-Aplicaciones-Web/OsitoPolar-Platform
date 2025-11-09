using OsitoPolarPlatform.API.Notifications.Domain.Model.Aggregates;
using OsitoPolarPlatform.API.Notifications.Domain.Model.Commands;

namespace OsitoPolarPlatform.API.Notifications.Domain.Services;

/// <summary>
/// Service for handling in-app notification commands
/// </summary>
public interface IInAppNotificationCommandService
{
    /// <summary>
    /// Create a new in-app notification
    /// </summary>
    Task<InAppNotification> Handle(CreateInAppNotificationCommand command);

    /// <summary>
    /// Mark a notification as read
    /// </summary>
    Task<InAppNotification?> Handle(MarkNotificationAsReadCommand command);

    /// <summary>
    /// Mark all notifications as read for a user
    /// </summary>
    Task Handle(MarkAllNotificationsAsReadCommand command);
}
