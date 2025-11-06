using OsitoPolarPlatform.API.Notifications.Domain.Model.Entities;
using OsitoPolarPlatform.API.Shared.Domain.Repositories;

namespace OsitoPolarPlatform.API.Notifications.Domain.Repositories;

/// <summary>
/// Repository for notification logs
/// </summary>
public interface INotificationRepository : IBaseRepository<NotificationLog>
{
    Task<IEnumerable<NotificationLog>> GetByRecipientAsync(string recipient);
    Task<IEnumerable<NotificationLog>> GetFailedNotificationsAsync();
}
