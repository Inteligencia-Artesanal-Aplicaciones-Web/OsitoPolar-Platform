using OsitoPolarPlatform.API.SubscriptionsAndPayments.Domain.Model.Aggregates;
using OsitoPolarPlatform.API.SubscriptionsAndPayments.Interfaces.REST.Resources;

namespace OsitoPolarPlatform.API.SubscriptionsAndPayments.Interfaces.REST.Transform;

public static class SubscriptionResourceFromEntityAssembler
{
    public static SubscriptionResource ToResourceFromEntity(Subscription subscription)
    {
        return new SubscriptionResource(
            subscription.Id,
            subscription.PlanName,
            subscription.Price.Amount,
            subscription.BillingCycle.ToString(),
            subscription.MaxEquipment,
            subscription.MaxClients,
            subscription.Features.Select(f => f.Name).ToList()
        );
    }

    public static IEnumerable<SubscriptionResource> ToResourceFromEntity(IEnumerable<Subscription> subscriptions)
    {
        return subscriptions.Select(ToResourceFromEntity);
    }
}