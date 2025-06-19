using OsitoPolarPlatform.API.SubscriptionsAndPayments.Domain.Model.Commands;

namespace OsitoPolarPlatform.API.SubscriptionsAndPayments.Interfaces.REST.Transform;

public static class UpgradeSubscriptionCommandFromResourceAssembler
{
    public static UpgradePlanCommand ToCommandFromResource(UpgradeSubscriptionResource resource)
    {
        return new UpgradePlanCommand(resource.UserId, resource.PlanId);
    }
}