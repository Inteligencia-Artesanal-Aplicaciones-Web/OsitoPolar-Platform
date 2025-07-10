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

    public async Task<Subscription?> Handle(CreateSubscriptionCommand command)
    {
        try
        {
            // Validaciones
            if (string.IsNullOrWhiteSpace(command.PlanName))
                throw new ArgumentException("Plan name is required.");
            if (command.Price < 0)
                throw new ArgumentException("Price must be greater than or equal to 0.");

            // Generar ID único
            var newId = await GenerateUniqueSubscriptionId();
            
            // Crear nueva suscripción
            var subscription = new Subscription(
                newId,
                command.PlanName,
                command.Price,
                command.BillingCycle,
                command.MaxEquipment,
                command.MaxClients,
                command.Features
            );

            // Guardar en repositorio
            await subscriptionRepository.AddAsync(subscription);
            await unitOfWork.CompleteAsync();

            Console.WriteLine($"Created subscription: {subscription.PlanName} with ID {subscription.Id}");
            return subscription;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating subscription: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> Handle(DeleteSubscriptionCommand command)
    {
        try
        {
            var subscription = await subscriptionRepository.FindByIdAsync(command.SubscriptionId);
            if (subscription == null)
            {
                Console.WriteLine($"Subscription {command.SubscriptionId} not found for deletion");
                return false;
            }
            
            subscriptionRepository.Remove(subscription);
            await unitOfWork.CompleteAsync();

            Console.WriteLine($"Deleted subscription: {subscription.PlanName} with ID {subscription.Id}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting subscription: {ex.Message}");
            return false;
        }
    }

    private async Task<int> GenerateUniqueSubscriptionId()
    {
        var random = new Random();
        int newId;
        Subscription? existingSubscription;
        
        do
        {
            newId = random.Next(1000, 9999);
            existingSubscription = await subscriptionRepository.FindByIdAsync(newId);
        } 
        while (existingSubscription != null);
        
        return newId;
    }
}