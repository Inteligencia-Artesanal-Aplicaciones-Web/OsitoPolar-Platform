using Microsoft.EntityFrameworkCore;
using EntityFrameworkCore.CreatedUpdatedDate.Extensions;
using OsitoPolarPlatform.API.SubscriptionsAndPayments.Infrastructure.Persistence.EFC.Configuration.Extensions;
using OsitoPolarPlatform.API.SubscriptionsAndPayments.Domain.Model.Aggregates;
using OsitoPolarPlatform.API.Shared.Infrastructure.Persistence.EFC.Configuration.Extensions;

namespace OsitoPolarPlatform.API.SubscriptionsAndPayments.Infrastructure.Persistence.EFC.Configuration;

/// <summary>
/// Subscriptions and Payments Bounded Context database context
/// </summary>
public class SubscriptionsDbContext(DbContextOptions<SubscriptionsDbContext> options) : DbContext(options)
{
    // Service Payments
    public DbSet<ServicePayment> ServicePayments { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        // Add the created and updated interceptor
        builder.AddCreatedUpdatedInterceptor();
        base.OnConfiguring(builder);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply Subscriptions and Payments context configuration
        builder.ApplySubscriptionsConfiguration();

        // Apply snake_case naming convention
        builder.UseSnakeCaseNamingConvention();
    }
}
