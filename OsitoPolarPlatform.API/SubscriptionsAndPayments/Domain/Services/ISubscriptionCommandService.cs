using OsitoPolarPlatform.API.SubscriptionsAndPayments.Domain.Model.Aggregates;
using OsitoPolarPlatform.API.SubscriptionsAndPayments.Domain.Model.Commands;

namespace OsitoPolarPlatform.API.SubscriptionsAndPayments.Domain.Services;

public interface ISubscriptionCommandService
{
    Task<Subscription?> Handle(UpgradePlanCommand command);
    Task<Subscription?> Handle(CreateSubscriptionCommand command);
    Task<bool> Handle(DeleteSubscriptionCommand command);
}