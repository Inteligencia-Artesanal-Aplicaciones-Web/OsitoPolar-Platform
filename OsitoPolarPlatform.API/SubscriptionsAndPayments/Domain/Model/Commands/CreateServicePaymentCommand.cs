namespace OsitoPolarPlatform.API.SubscriptionsAndPayments.Domain.Model.Commands;

/// <summary>
/// Command to create a payment for a completed service
/// </summary>
public record CreateServicePaymentCommand(
    int WorkOrderId,
    int ServiceRequestId,
    int OwnerId,
    int ProviderId,
    decimal TotalAmount,
    string Description);
