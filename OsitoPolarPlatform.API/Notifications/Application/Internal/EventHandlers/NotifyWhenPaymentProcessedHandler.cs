using OsitoPolarPlatform.API.SubscriptionsAndPayments.Domain.Model.Events;
using OsitoPolarPlatform.API.Shared.Application.Internal.EventHandlers;
using OsitoPolarPlatform.API.Notifications.Interfaces.ACL;

namespace OsitoPolarPlatform.API.Notifications.Application.Internal.EventHandlers;

/// <summary>
/// Event handler that sends notifications when a payment is processed
/// This demonstrates async communication from SubscriptionsAndPayments BC to Notifications BC
/// </summary>
public class NotifyWhenPaymentProcessedHandler : IEventHandler<PaymentProcessedEvent>
{
    private readonly INotificationContextFacade _notificationFacade;
    private readonly ILogger<NotifyWhenPaymentProcessedHandler> _logger;

    public NotifyWhenPaymentProcessedHandler(
        INotificationContextFacade notificationFacade,
        ILogger<NotifyWhenPaymentProcessedHandler> logger)
    {
        _notificationFacade = notificationFacade;
        _logger = logger;
    }

    public async Task Handle(PaymentProcessedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "[Notifications BC] Payment processed event received - Type: {Type}, Amount: ${Amount}",
            notification.PaymentType, notification.Amount);

        // Send notification based on payment type
        if (notification.PaymentType == "Subscription")
        {
            var message = $"Your subscription payment of ${notification.Amount:F2} has been processed successfully.";
            await _notificationFacade.CreateInAppNotification(
                notification.UserId,
                "✅ Payment Confirmed",
                message);

            _logger.LogInformation(
                "[Notifications BC] Subscription payment notification sent to user {UserId}",
                notification.UserId);
        }
        else if (notification.PaymentType == "Service" && notification.ProviderId.HasValue)
        {
            // Notify provider about received payment
            var message = $"Payment of ${notification.ProviderAmount:F2} received from service payment (Total: ${notification.Amount:F2}).";
            await _notificationFacade.CreateInAppNotification(
                notification.ProviderId.Value,
                "💰 Payment Received",
                message);

            _logger.LogInformation(
                "[Notifications BC] Service payment notification sent to provider {ProviderId}",
                notification.ProviderId.Value);
        }

        // Future: Could also trigger analytics updates, email notifications, etc.
    }
}
