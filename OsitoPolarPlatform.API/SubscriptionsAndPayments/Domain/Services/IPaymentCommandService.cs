using OsitoPolarPlatform.API.SubscriptionsAndPayments.Domain.Model.Aggregates;
using OsitoPolarPlatform.API.SubscriptionsAndPayments.Domain.Model.Commands;

namespace OsitoPolarPlatform.API.SubscriptionsAndPayments.Domain.Services;

public interface IPaymentCommandService
{
    Task<(Payment payment, string checkoutUrl)> Handle(CreatePaymentSessionCommand command);
    Task<Payment?> Handle(ProcessPaymentWebhookCommand command);
}