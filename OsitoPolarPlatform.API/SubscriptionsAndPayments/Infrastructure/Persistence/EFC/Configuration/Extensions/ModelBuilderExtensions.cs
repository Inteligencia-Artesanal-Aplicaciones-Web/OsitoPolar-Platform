using Microsoft.EntityFrameworkCore;
using OsitoPolarPlatform.API.SubscriptionsAndPayments.Domain.Model.Aggregates;
using OsitoPolarPlatform.API.SubscriptionsAndPayments.Domain.Model.ValueObjects;

namespace OsitoPolarPlatform.API.SubscriptionsAndPayments.Infrastructure.Persistence.EFC.Configuration.Extensions;

/// <summary>
/// Extensions for configuring Subscriptions context entities - MINIMAL VERSION
/// </summary>
public static class ModelBuilderExtensions
{
    public static void ApplySubscriptionsConfiguration(this ModelBuilder builder)
    {

        builder.Entity<Subscription>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Id).IsRequired().ValueGeneratedOnAdd();
            entity.Property(s => s.PlanName).HasMaxLength(200).IsRequired();
            entity.Property(s => s.BillingCycle).IsRequired().HasConversion<string>();
            entity.Property(s => s.MaxEquipment).IsRequired(false);
            entity.Property(s => s.MaxClients).IsRequired(false);
            
            entity.Property(s => s.CreatedDate).HasColumnName("created_at");
            entity.Property(s => s.UpdatedDate).HasColumnName("updated_at");
            

            entity.Ignore(s => s.Features);
            entity.Ignore(s => s.FeaturesJson);
            entity.Ignore(s => s.Price);
            
            entity.ToTable("subscriptions");
        });

        // Seed MINIMAL data
        SeedMinimalSubscriptionPlans(builder);
    }

    private static void SeedMinimalSubscriptionPlans(ModelBuilder builder)
    {
        var now = DateTimeOffset.UtcNow;

        builder.Entity<Subscription>().HasData(
            new 
            {
                Id = 1,
                PlanName = "Basic (Polar Bear) - $18.99/month - Up to 6 units",
                BillingCycle = BillingCycle.Monthly,
                MaxEquipment = 6,
                MaxClients = (int?)null,
                CreatedDate = now,
                UpdatedDate = now
            },
            new 
            {
                Id = 2,
                PlanName = "Standard (Snow Bear) - $35.13/month - Up to 12 units",
                BillingCycle = BillingCycle.Monthly,
                MaxEquipment = 12,
                MaxClients = (int?)null,
                CreatedDate = now,
                UpdatedDate = now
            },
            new 
            {
                Id = 3,
                PlanName = "Premium (Glacial Bear) - $67.56/month - Up to 24 units",
                BillingCycle = BillingCycle.Monthly,
                MaxEquipment = 24,
                MaxClients = (int?)null,
                CreatedDate = now,
                UpdatedDate = now
            },
            new 
            {
                Id = 4,
                PlanName = "Small Company - $40.51/month - Up to 10 clients",
                BillingCycle = BillingCycle.Monthly,
                MaxEquipment = (int?)null,
                MaxClients = 10,
                CreatedDate = now,
                UpdatedDate = now
            },
            new 
            {
                Id = 5,
                PlanName = "Medium Company - $81.08/month - Up to 30 clients",
                BillingCycle = BillingCycle.Monthly,
                MaxEquipment = (int?)null,
                MaxClients = 30,
                CreatedDate = now,
                UpdatedDate = now
            },
            new 
            {
                Id = 6,
                PlanName = "Enterprise Premium - $162.16/month - Unlimited clients",
                BillingCycle = BillingCycle.Monthly,
                MaxEquipment = (int?)null,
                MaxClients = 999999,
                CreatedDate = now,
                UpdatedDate = now
            }
        );
    }
}