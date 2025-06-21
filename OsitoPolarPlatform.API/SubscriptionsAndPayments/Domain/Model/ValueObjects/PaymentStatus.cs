namespace OsitoPolarPlatform.API.SubscriptionsAndPayments.Domain.Model.ValueObjects;

public enum PaymentStatus
{
    Pending = 1,
    Processing = 2,
    Succeeded = 3,
    Failed = 4,
    Cancelled = 5
}