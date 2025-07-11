using OsitoPolarPlatform.API.SubscriptionsAndPayments.Domain.Model.ValueObjects;

namespace OsitoPolarPlatform.API.SubscriptionsAndPayments.Domain.Model.Commands;

public record CreateSubscriptionCommand(
    string PlanName,
    decimal Price,
    BillingCycle BillingCycle,
    int? MaxEquipment,
    int? MaxClients,
    List<string>? Features
);