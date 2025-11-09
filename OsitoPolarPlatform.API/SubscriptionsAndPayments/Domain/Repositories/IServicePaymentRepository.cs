using OsitoPolarPlatform.API.SubscriptionsAndPayments.Domain.Model.Aggregates;

namespace OsitoPolarPlatform.API.SubscriptionsAndPayments.Domain.Repositories;

/// <summary>
/// Repository for ServicePayment aggregate
/// </summary>
public interface IServicePaymentRepository
{
    Task<ServicePayment?> FindByIdAsync(int id);
    Task<ServicePayment?> FindByWorkOrderIdAsync(int workOrderId);
    Task<IEnumerable<ServicePayment>> FindByOwnerIdAsync(int ownerId);
    Task<IEnumerable<ServicePayment>> FindByProviderIdAsync(int providerId);
    Task AddAsync(ServicePayment servicePayment);
    void Update(ServicePayment servicePayment);
}
