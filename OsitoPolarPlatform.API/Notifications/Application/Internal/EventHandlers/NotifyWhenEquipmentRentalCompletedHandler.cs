using OsitoPolarPlatform.API.EquipmentManagement.Domain.Model.Events;
using OsitoPolarPlatform.API.Shared.Application.Internal.EventHandlers;
using OsitoPolarPlatform.API.Notifications.Interfaces.ACL;
using OsitoPolarPlatform.API.Profiles.Interfaces.ACL;

namespace OsitoPolarPlatform.API.Notifications.Application.Internal.EventHandlers;

/// <summary>
/// Event handler that sends notifications when an equipment rental is completed
/// This demonstrates async communication from EquipmentManagement BC to Notifications BC
/// </summary>
public class NotifyWhenEquipmentRentalCompletedHandler : IEventHandler<EquipmentRentalCompletedEvent>
{
    private readonly INotificationContextFacade _notificationFacade;
    private readonly IProfilesContextFacade _profilesFacade;
    private readonly ILogger<NotifyWhenEquipmentRentalCompletedHandler> _logger;

    public NotifyWhenEquipmentRentalCompletedHandler(
        INotificationContextFacade notificationFacade,
        IProfilesContextFacade profilesFacade,
        ILogger<NotifyWhenEquipmentRentalCompletedHandler> logger)
    {
        _notificationFacade = notificationFacade;
        _profilesFacade = profilesFacade;
        _logger = logger;
    }

    public async Task Handle(EquipmentRentalCompletedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "[Notifications BC] Equipment rental completed event received - Equipment: {EquipmentName}, Duration: {Months} months",
            notification.EquipmentName, notification.RentalDurationMonths);

        // Notify the renter (owner) about successful rental
        var renterMessage = $"You have successfully rented {notification.EquipmentName} for {notification.RentalDurationMonths} month(s). " +
                           $"Total: ${notification.TotalAmount:F2}. Rental ends on {notification.RentalEndDate:yyyy-MM-dd}.";

        await _notificationFacade.CreateInAppNotification(
            notification.OwnerId,
            "🎉 Equipment Rental Confirmed",
            renterMessage);

        _logger.LogInformation(
            "[Notifications BC] Rental confirmation sent to owner {OwnerId}",
            notification.OwnerId);

        // Notify the provider about payment received
        var providerUserId = await _profilesFacade.GetProviderUserIdByProviderId(notification.ProviderId);
        if (providerUserId > 0)
        {
            var providerMessage = $"Payment of ${notification.ProviderAmount:F2} received for rental of {notification.EquipmentName} " +
                                 $"({notification.RentalDurationMonths} months). Platform fee: ${notification.PlatformFee:F2}.";

            await _notificationFacade.CreateInAppNotification(
                providerUserId,
                "💰 Rental Payment Received",
                providerMessage);

            _logger.LogInformation(
                "[Notifications BC] Rental payment notification sent to provider {ProviderId}",
                notification.ProviderId);
        }

        // Future: Could trigger analytics, generate reports, schedule rental end reminders, etc.
    }
}
