using OsitoPolarPlatform.API.Notifications.Domain.Model.Aggregates;
using OsitoPolarPlatform.API.Shared.Domain.Repositories;

namespace OsitoPolarPlatform.API.Notifications.Domain.Repositories;

/// <summary>
/// Repository for in-app notifications
/// </summary>
public interface IInAppNotificationRepository : IBaseRepository<InAppNotification>
{
    /// <summary>
    /// Get all notifications for a specific user, ordered by timestamp descending
    /// </summary>
    Task<IEnumerable<InAppNotification>> GetByUserIdAsync(int userId);

    /// <summary>
    /// Get unread notifications for a specific user
    /// </summary>
    Task<IEnumerable<InAppNotification>> GetUnreadByUserIdAsync(int userId);

    /// <summary>
    /// Get count of unread notifications for a user
    /// </summary>
    Task<int> GetUnreadCountByUserIdAsync(int userId);

    /// <summary>
    /// Mark all notifications as read for a specific user
    /// </summary>
    Task MarkAllAsReadForUserAsync(int userId);

    /// <summary>
    /// Find a notification by ID and verify it belongs to the user
    /// </summary>
    Task<InAppNotification?> FindByIdAndUserIdAsync(int notificationId, int userId);
}
