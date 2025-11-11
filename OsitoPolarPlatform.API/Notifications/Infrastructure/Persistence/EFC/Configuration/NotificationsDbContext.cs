using Microsoft.EntityFrameworkCore;
using EntityFrameworkCore.CreatedUpdatedDate.Extensions;
using OsitoPolarPlatform.API.Notifications.Infrastructure.Persistence.EFC.Configuration.Extensions;
using OsitoPolarPlatform.API.Notifications.Domain.Model.Entities;
using OsitoPolarPlatform.API.Shared.Infrastructure.Persistence.EFC.Configuration.Extensions;

namespace OsitoPolarPlatform.API.Notifications.Infrastructure.Persistence.EFC.Configuration;

/// <summary>
/// Notifications Bounded Context database context
/// </summary>
public class NotificationsDbContext(DbContextOptions<NotificationsDbContext> options) : DbContext(options)
{
    // Notifications
    public DbSet<NotificationLog> NotificationLogs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        // Add the created and updated interceptor
        builder.AddCreatedUpdatedInterceptor();
        base.OnConfiguring(builder);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply Notifications context configuration
        builder.ApplyNotificationsConfiguration();

        // Apply snake_case naming convention
        builder.UseSnakeCaseNamingConvention();
    }
}
