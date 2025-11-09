using OsitoPolarPlatform.API.Notifications.Domain.Model.Aggregates;
using OsitoPolarPlatform.API.Notifications.Domain.Model.Queries;

namespace OsitoPolarPlatform.API.Notifications.Domain.Services;

/// <summary>
/// Service for handling in-app notification queries
/// </summary>
public interface IInAppNotificationQueryService
{
    /// <summary>
    /// Get all notifications for a user
    /// </summary>
    Task<IEnumerable<InAppNotification>> Handle(GetNotificationsByUserIdQuery query);

    /// <summary>
    /// Get unread notification count for a user
    /// </summary>
    Task<int> Handle(GetUnreadNotificationCountQuery query);

    /// <summary>
    /// Get a specific notification by ID
    /// </summary>
    Task<InAppNotification?> Handle(GetNotificationByIdQuery query);
}
