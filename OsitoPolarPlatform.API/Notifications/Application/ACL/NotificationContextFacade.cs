using OsitoPolarPlatform.API.Notifications.Domain.Repositories;
using OsitoPolarPlatform.API.Notifications.Domain.Model.Aggregates;
using OsitoPolarPlatform.API.Notifications.Application.Internal.CommandServices;
using OsitoPolarPlatform.API.Notifications.Interfaces.ACL;
using OsitoPolarPlatform.API.Shared.Domain.Repositories;

namespace OsitoPolarPlatform.API.Notifications.Application.ACL;

/// <summary>
/// Facade implementation for the Notifications context
/// </summary>
/// <param name="emailCommandService">The email command service</param>
/// <param name="inAppNotificationRepository">The in-app notification repository</param>
/// <param name="unitOfWork">The unit of work</param>
public class NotificationContextFacade(
    IEmailCommandService emailCommandService,
    IInAppNotificationRepository inAppNotificationRepository,
    IUnitOfWork unitOfWork) : INotificationContextFacade
{
    public async Task<bool> SendEmailNotification(string to, string subject, string body)
    {
        try
        {
            await emailCommandService.SendRawEmailAsync(to, "", subject, body);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<int> CreateInAppNotification(int userId, string title, string message)
    {
        var notification = new InAppNotification(
            userId: userId,
            title: title,
            message: message,
            type: "system",
            severity: "info"
        );

        await inAppNotificationRepository.AddAsync(notification);
        await unitOfWork.CompleteAsync();
        return notification.Id;
    }

    public async Task<bool> MarkNotificationAsRead(int notificationId)
    {
        var notification = await inAppNotificationRepository.FindByIdAsync(notificationId);
        if (notification == null) return false;

        notification.MarkAsRead();
        inAppNotificationRepository.Update(notification);
        await unitOfWork.CompleteAsync();
        return true;
    }
}
