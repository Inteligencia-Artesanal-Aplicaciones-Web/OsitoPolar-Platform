using OsitoPolarPlatform.API.SubscriptionsAndPayments.Domain.Model.Aggregates;
using OsitoPolarPlatform.API.SubscriptionsAndPayments.Domain.Model.Commands;
using OsitoPolarPlatform.API.SubscriptionsAndPayments.Domain.Repositories;
using OsitoPolarPlatform.API.SubscriptionsAndPayments.Domain.Services;
using OsitoPolarPlatform.API.Shared.Domain.Repositories;

namespace OsitoPolarPlatform.API.SubscriptionsAndPayments.Application.Internal.CommandServices;

public class SubscriptionCommandService(
    ISubscriptionRepository subscriptionRepository,
    IUnitOfWork unitOfWork) : ISubscriptionCommandService
{
    public async Task<Subscription?> Handle(UpgradePlanCommand command)
    {
        var subscription = await subscriptionRepository.FindByUserIdAsync(command.UserId)
                           ?? throw new InvalidOperationException("Subscription not found");

        var newPlan = await subscriptionRepository.FindByIdAsync(command.PlanId)
                      ?? throw new InvalidOperationException("Plan not found");

        subscription.UpdatePlan(newPlan.PlanName, newPlan.Price.Amount, newPlan.BillingCycle, newPlan.MaxEquipment, newPlan.MaxClients, 
            newPlan.Features.Select(f => f.Name).ToList());

        subscriptionRepository.Update(subscription);
        await unitOfWork.CompleteAsync();

        return subscription;
    }
}