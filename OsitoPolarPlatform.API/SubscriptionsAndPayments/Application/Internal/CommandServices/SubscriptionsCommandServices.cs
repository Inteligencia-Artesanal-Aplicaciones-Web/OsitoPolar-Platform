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
        try
        {
            
            var newPlan = await subscriptionRepository.FindByIdAsync(command.PlanId)
                          ?? throw new InvalidOperationException($"Plan {command.PlanId} not found");
            
            Console.WriteLine($"User {command.UserId} upgraded to plan {newPlan.PlanName}");
            
            // TODO: Implement real logic to update the user's subscription in the database
            
            return newPlan;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in upgrade: {ex.Message}");
            
            return null;
        }
    }
}