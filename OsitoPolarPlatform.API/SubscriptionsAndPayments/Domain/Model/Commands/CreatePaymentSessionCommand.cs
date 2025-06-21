namespace OsitoPolarPlatform.API.SubscriptionsAndPayments.Domain.Model.Commands;

public record CreatePaymentSessionCommand(int UserId, int PlanId, string SuccessUrl, string CancelUrl);