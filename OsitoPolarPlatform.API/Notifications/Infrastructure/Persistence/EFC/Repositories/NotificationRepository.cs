using Microsoft.EntityFrameworkCore;
using OsitoPolarPlatform.API.Notifications.Domain.Model.Entities;
using OsitoPolarPlatform.API.Notifications.Domain.Repositories;
using OsitoPolarPlatform.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using OsitoPolarPlatform.API.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace OsitoPolarPlatform.API.Notifications.Infrastructure.Persistence.EFC.Repositories;

/// <summary>
/// Repository implementation for notification logs
/// </summary>
public class NotificationRepository : BaseRepository<NotificationLog>, INotificationRepository
{
    public NotificationRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<NotificationLog>> GetByRecipientAsync(string recipient)
    {
        return await Context.Set<NotificationLog>()
            .Where(n => n.Recipient == recipient)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<NotificationLog>> GetFailedNotificationsAsync()
    {
        return await Context.Set<NotificationLog>()
            .Where(n => n.Status == Domain.Model.ValueObjects.NotificationStatus.Failed)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }
}
