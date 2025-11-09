using Microsoft.EntityFrameworkCore;
using OsitoPolarPlatform.API.Notifications.Domain.Model.Aggregates;
using OsitoPolarPlatform.API.Notifications.Domain.Repositories;
using OsitoPolarPlatform.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using OsitoPolarPlatform.API.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace OsitoPolarPlatform.API.Notifications.Infrastructure.Persistence.EFC.Repositories;

/// <summary>
/// Repository implementation for in-app notifications
/// </summary>
public class InAppNotificationRepository : BaseRepository<InAppNotification>, IInAppNotificationRepository
{
    public InAppNotificationRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<InAppNotification>> GetByUserIdAsync(int userId)
    {
        return await Context.Set<InAppNotification>()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.Timestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<InAppNotification>> GetUnreadByUserIdAsync(int userId)
    {
        return await Context.Set<InAppNotification>()
            .Where(n => n.UserId == userId && !n.IsRead)
            .OrderByDescending(n => n.Timestamp)
            .ToListAsync();
    }

    public async Task<int> GetUnreadCountByUserIdAsync(int userId)
    {
        return await Context.Set<InAppNotification>()
            .CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    public async Task MarkAllAsReadForUserAsync(int userId)
    {
        var unreadNotifications = await Context.Set<InAppNotification>()
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in unreadNotifications)
        {
            notification.MarkAsRead();
        }

        await Context.SaveChangesAsync();
    }

    public async Task<InAppNotification?> FindByIdAndUserIdAsync(int notificationId, int userId)
    {
        return await Context.Set<InAppNotification>()
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);
    }
}
