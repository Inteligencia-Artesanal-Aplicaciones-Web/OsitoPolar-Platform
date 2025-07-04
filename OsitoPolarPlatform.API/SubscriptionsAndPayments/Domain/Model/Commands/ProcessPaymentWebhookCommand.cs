namespace OsitoPolarPlatform.API.SubscriptionsAndPayments.Domain.Model.Commands;

public record ProcessPaymentWebhookCommand(string StripeSessionId, string EventType);