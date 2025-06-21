using Microsoft.EntityFrameworkCore;
using OsitoPolarPlatform.API.SubscriptionsAndPayments.Domain.Model.Aggregates;
using OsitoPolarPlatform.API.SubscriptionsAndPayments.Domain.Repositories;
using OsitoPolarPlatform.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using OsitoPolarPlatform.API.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace OsitoPolarPlatform.API.SubscriptionsAndPayments.Infrastructure.Persistence.EFC.Repositories;

public class PaymentRepository : BaseRepository<Payment>, IPaymentRepository
{
    public PaymentRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Payment?> FindByStripeSessionIdAsync(string stripeSessionId)
    {
        return await Context.Set<Payment>()
            .Where(p => EF.Property<string>(p, "StripeSessionId") == stripeSessionId)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Payment>> FindByUserIdAsync(int userId)
    {
        return await Context.Set<Payment>()
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedDate)
            .ToListAsync();
    }
}