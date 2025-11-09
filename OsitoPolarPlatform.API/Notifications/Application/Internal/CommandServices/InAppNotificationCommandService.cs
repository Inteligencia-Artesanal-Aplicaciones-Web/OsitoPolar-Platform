using OsitoPolarPlatform.API.Notifications.Domain.Model.Aggregates;
using OsitoPolarPlatform.API.Notifications.Domain.Model.Commands;
using OsitoPolarPlatform.API.Notifications.Domain.Repositories;
using OsitoPolarPlatform.API.Notifications.Domain.Services;
using OsitoPolarPlatform.API.Shared.Domain.Repositories;

namespace OsitoPolarPlatform.API.Notifications.Application.Internal.CommandServices;

/// <summary>
/// Command service for in-app notifications
/// </summary>
public class InAppNotificationCommandService : IInAppNotificationCommandService
{
    private readonly IInAppNotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<InAppNotificationCommandService> _logger;

    public InAppNotificationCommandService(
        IInAppNotificationRepository notificationRepository,
        IUnitOfWork unitOfWork,
        ILogger<InAppNotificationCommandService> logger)
    {
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<InAppNotification> Handle(CreateInAppNotificationCommand command)
    {
        _logger.LogInformation(
            "Creating notification for user {UserId}: {Title}",
            command.UserId,
            command.Title
        );

        var notification = new InAppNotification(
            command.UserId,
            command.Title,
            command.Message,
            command.Type,
            command.Severity,
            command.EquipmentId,
            command.ServiceRequestId,
            command.Metadata
        );

        await _notificationRepository.AddAsync(notification);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation(
            "Notification {NotificationId} created successfully for user {UserId}",
            notification.Id,
            command.UserId
        );

        return notification;
    }

    public async Task<InAppNotification?> Handle(MarkNotificationAsReadCommand command)
    {
        _logger.LogInformation(
            "Marking notification {NotificationId} as read for user {UserId}",
            command.NotificationId,
            command.UserId
        );

        var notification = await _notificationRepository.FindByIdAndUserIdAsync(
            command.NotificationId,
            command.UserId
        );

        if (notification == null)
        {
            _logger.LogWarning(
                "Notification {NotificationId} not found for user {UserId}",
                command.NotificationId,
                command.UserId
            );
            return null;
        }

        notification.MarkAsRead();
        _notificationRepository.Update(notification);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation(
            "Notification {NotificationId} marked as read",
            command.NotificationId
        );

        return notification;
    }

    public async Task Handle(MarkAllNotificationsAsReadCommand command)
    {
        _logger.LogInformation(
            "Marking all notifications as read for user {UserId}",
            command.UserId
        );

        await _notificationRepository.MarkAllAsReadForUserAsync(command.UserId);

        _logger.LogInformation(
            "All notifications marked as read for user {UserId}",
            command.UserId
        );
    }
}
