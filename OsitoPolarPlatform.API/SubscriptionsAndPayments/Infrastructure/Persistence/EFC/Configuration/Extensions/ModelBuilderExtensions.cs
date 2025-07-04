using OsitoPolarPlatform.API.SubscriptionsAndPayments.Domain.Model.Aggregates;
using Microsoft.EntityFrameworkCore;
using OsitoPolarPlatform.API.SubscriptionsAndPayments.Domain.Model.ValueObjects;

namespace OsitoPolarPlatform.API.SubscriptionsAndPayments.Infrastructure.Persistence.EFC.Configuration.Extensions;

public static class ModelBuilderExtensions
{
    public static void ApplySubscriptionsConfiguration(this ModelBuilder builder)
    {
        // Subscription configuration
        builder.Entity<Subscription>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd();
            entity.ToTable("subscriptions");
            
            entity.Property(s => s.PlanName)
                .HasColumnName("plan_name")
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(s => s.Price)
                .HasConversion(
                    p => p.Amount,
                    v => new Price(v, "USD"))
                .HasColumnName("price")
                .HasColumnType("decimal(18,2)")
                .IsRequired();
            
            entity.Property(s => s.BillingCycle)
                .HasConversion(
                    b => b.ToString(), 
                    v => (BillingCycle)Enum.Parse(typeof(BillingCycle), v))
                .HasColumnName("billing_cycle")
                .IsRequired()
                .HasMaxLength(20);
                
            entity.Property(s => s.MaxEquipment).HasColumnName("max_equipment");
            entity.Property(s => s.MaxClients).HasColumnName("max_clients");
            
            entity.Property<string>("FeaturesJson")
                .HasColumnName("features")
                .HasColumnType("json")
                .IsRequired(false);
                
            entity.Ignore(s => s.Features);
            
            entity.Property<DateTimeOffset?>("CreatedDate")
                .HasColumnName("created_date")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            entity.Property<DateTimeOffset?>("UpdatedDate")
                .HasColumnName("updated_date")
                .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");
        });
        
        builder.Entity<Payment>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd();
            entity.ToTable("payments");
            
            entity.Property(p => p.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(p => p.SubscriptionId).HasColumnName("subscription_id").IsRequired();
            
            entity.Property(p => p.Amount)
                .HasConversion(
                    a => a.Amount,
                    v => new Price(v, "USD"))
                .HasColumnName("amount")
                .HasColumnType("decimal(10,2)")
                .IsRequired();
            
            entity.Property(p => p.StripeSession)
                .HasConversion(
                    s => s.SessionId,
                    v => new StripeSession(v))
                .HasColumnName("stripe_session_id")
                .HasMaxLength(255)
                .IsRequired();
            
            entity.Property(p => p.CustomerEmail)
                .HasColumnName("customer_email")
                .HasMaxLength(255)
                .IsRequired(false);
                
            entity.Property(p => p.Description)
                .HasColumnName("description")
                .HasMaxLength(500)
                .IsRequired(false);
            
                        
            entity.Property<DateTimeOffset?>("CreatedDate")
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            entity.Property<DateTimeOffset?>("UpdatedDate")
                .HasColumnName("updated_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");
        });
    }
}