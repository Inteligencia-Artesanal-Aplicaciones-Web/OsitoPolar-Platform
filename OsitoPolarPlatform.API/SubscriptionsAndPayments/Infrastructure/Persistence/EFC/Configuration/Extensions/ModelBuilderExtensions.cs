using OsitoPolarPlatform.API.SubscriptionsAndPayments.Domain.Model.Aggregates;
using Microsoft.EntityFrameworkCore;
using OsitoPolarPlatform.API.SubscriptionsAndPayments.Domain.Model.ValueObjects;

namespace OsitoPolarPlatform.API.SubscriptionsAndPayments.Infrastructure.Persistence.EFC.Configuration.Extensions;

/// <summary>
/// Extensions for configuring the Entity Framework Core model builder for the Subscriptions and Payments context.
/// </summary>
public static class ModelBuilderExtensions
{
    /// <summary>
    /// Applies the configuration for the Subscriptions and Payments context entities.
    /// </summary>
    /// <param name="builder">
    /// The <see cref="ModelBuilder"/> instance to apply the configuration to.
    /// </param>
    public static void ApplySubscriptionsConfiguration(this ModelBuilder builder)
    {
        builder.Entity<Subscription>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Id).IsRequired().ValueGeneratedOnAdd();
            entity.Property(s => s.PlanName).IsRequired().HasMaxLength(100);
            entity.Property(s => s.Price).HasConversion(p => p.Amount, v => new Price(v, "USD")).IsRequired().HasColumnType("decimal(18,2)");
            entity.Property(s => s.BillingCycle).HasConversion(b => b.ToString(), v => (BillingCycle)Enum.Parse(typeof(BillingCycle), v)).IsRequired().HasMaxLength(20);
            entity.Property(s => s.MaxEquipment);
            entity.Property(s => s.MaxClients);
            entity.OwnsMany(s => s.Features, f =>
            {
                f.ToTable("subscription_features");
                f.WithOwner().HasForeignKey("subscription_id"); // Foreign key to Subscription
                f.Property<int>("subscription_id").IsRequired(); // Ensure non-nullable foreign key
                f.Property<string>("Name").HasMaxLength(200).IsRequired(); // Explicitly map Name property
                f.HasKey("subscription_id", "Name"); // Composite primary key
            });
            
        });
        builder.Entity<Subscription>().Property(wo => wo.CreatedDate).HasColumnName("CreatedAt");
        builder.Entity<Subscription>().Property(wo => wo.UpdatedDate).HasColumnName("UpdatedAt");
    }
}