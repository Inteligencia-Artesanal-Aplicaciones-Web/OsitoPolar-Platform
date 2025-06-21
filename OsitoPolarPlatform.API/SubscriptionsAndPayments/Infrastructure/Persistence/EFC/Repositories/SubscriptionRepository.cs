using OsitoPolarPlatform.API.SubscriptionsAndPayments.Domain.Model.Aggregates;
using OsitoPolarPlatform.API.SubscriptionsAndPayments.Domain.Repositories;
using OsitoPolarPlatform.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using OsitoPolarPlatform.API.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace OsitoPolarPlatform.API.SubscriptionsAndPayments.Infrastructure.Persistence.EFC.Repositories;

public class SubscriptionRepository : BaseRepository<Subscription>, ISubscriptionRepository
{
    public SubscriptionRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Subscription?> FindByUserIdAsync(int userId)
    {
        // SOLUCIÓN TEMPORAL: Para que funcione el flujo inicial
        // En un sistema real, necesitarías una tabla UserSubscription o similar
        
        // Por ahora, retorna null para usuarios que no tienen suscripción activa
        // Esto permitirá que el flujo de pago funcione correctamente
        return null;
        
        
        // return await Context.Set<Subscription>().FirstOrDefaultAsync(s => s.Id == 1);
    }
}