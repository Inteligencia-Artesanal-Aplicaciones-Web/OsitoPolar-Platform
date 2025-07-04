using OsitoPolarPlatform.API.SubscriptionsAndPayments.Domain.Model.Aggregates;
using OsitoPolarPlatform.API.Shared.Domain.Repositories;

namespace OsitoPolarPlatform.API.SubscriptionsAndPayments.Domain.Repositories;

public interface IPaymentRepository : IBaseRepository<Payment>
{
    Task<Payment?> FindByStripeSessionIdAsync(string stripeSessionId);
    Task<IEnumerable<Payment>> FindByUserIdAsync(int userId);
}