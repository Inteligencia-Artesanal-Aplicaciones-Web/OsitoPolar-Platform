using Microsoft.EntityFrameworkCore;
using EntityFrameworkCore.CreatedUpdatedDate.Extensions;
using OsitoPolarPlatform.API.Analytics.Infrastructure.Persistence.EFC.Configuration.Extensions;
using OsitoPolarPlatform.API.EquipmentManagement.Infrastructure.Persistence.EFC.Configuration.Extensions;
using OsitoPolarPlatform.API.ServiceRequests.Infrastructure.Persistence.EFC.Configuration.Extensions;
using OsitoPolarPlatform.API.Shared.Infrastructure.Persistence.EFC.Configuration.Extensions;
using OsitoPolarPlatform.API.WorkOrders.Infrastructure.Persistence.EFC.Configuration.Extensions;
using OsitoPolarPlatform.API.bc_technicians.Infrastructure.Persistence.EFC.Configuration.Extensions;
using OsitoPolarPlatform.API.IAM.Infrastructure.Persistence.EFC.Configuration.Extensions;
using OsitoPolarPlatform.API.Profiles.Infrastructure.Persistence.EFC.Configuration.Extensions;
using OsitoPolarPlatform.API.SubscriptionsAndPayments.Infrastructure.Persistence.EFC.Configuration.Extensions;

namespace OsitoPolarPlatform.API.Shared.Infrastructure.Persistence.EFC.Configuration;

/// <summary>
/// Application database context
/// </summary>
public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        // Add the created and updated interceptor
        builder.AddCreatedUpdatedInterceptor();
        base.OnConfiguring(builder);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // Apply ALL context configurations FIRST (in order)
        builder.ApplyEquipmentConfiguration();       // Equipment Management
        builder.ApplyAnalyticsConfiguration();       // Analytics 
        builder.ApplyServiceRequestConfiguration();  // Service Requests
        builder.ApplyWorkOrderConfiguration();       // Work Orders
        builder.ApplyTechnicianConfiguration();      // Technicians
        builder.ApplySubscriptionsConfiguration();  // Subscriptions and Payments
        // Apply snake_case naming convention LAST (only once!)
        
        
        // Apply Profiles context configuration
        builder.ApplyProfilesConfiguration();

        // Apply IAM context configuration
        builder.ApplyIamConfiguration();
        builder.UseSnakeCaseNamingConvention();
    }
}