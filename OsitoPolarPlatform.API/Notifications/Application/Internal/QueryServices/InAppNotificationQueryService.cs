using OsitoPolarPlatform.API.Notifications.Domain.Model.Aggregates;
using OsitoPolarPlatform.API.Notifications.Domain.Model.Queries;
using OsitoPolarPlatform.API.Notifications.Domain.Repositories;
using OsitoPolarPlatform.API.Notifications.Domain.Services;

namespace OsitoPolarPlatform.API.Notifications.Application.Internal.QueryServices;

/// <summary>
/// Query service for in-app notifications
/// </summary>
public class InAppNotificationQueryService : IInAppNotificationQueryService
{
    private readonly IInAppNotificationRepository _notificationRepository;
    private readonly ILogger<InAppNotificationQueryService> _logger;

    public InAppNotificationQueryService(
        IInAppNotificationRepository notificationRepository,
        ILogger<InAppNotificationQueryService> logger)
    {
        _notificationRepository = notificationRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<InAppNotification>> Handle(GetNotificationsByUserIdQuery query)
    {
        _logger.LogInformation("Getting notifications for user {UserId}", query.UserId);
        return await _notificationRepository.GetByUserIdAsync(query.UserId);
    }

    public async Task<int> Handle(GetUnreadNotificationCountQuery query)
    {
        _logger.LogInformation("Getting unread notification count for user {UserId}", query.UserId);
        return await _notificationRepository.GetUnreadCountByUserIdAsync(query.UserId);
    }

    public async Task<InAppNotification?> Handle(GetNotificationByIdQuery query)
    {
        _logger.LogInformation("Getting notification {NotificationId}", query.NotificationId);
        return await _notificationRepository.FindByIdAsync(query.NotificationId);
    }
}
