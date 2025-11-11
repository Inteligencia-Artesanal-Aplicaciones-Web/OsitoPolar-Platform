using Microsoft.EntityFrameworkCore;
using EntityFrameworkCore.CreatedUpdatedDate.Extensions;
using OsitoPolarPlatform.API.WorkOrders.Infrastructure.Persistence.EFC.Configuration.Extensions;
using OsitoPolarPlatform.API.Shared.Infrastructure.Persistence.EFC.Configuration.Extensions;

namespace OsitoPolarPlatform.API.WorkOrders.Infrastructure.Persistence.EFC.Configuration;

/// <summary>
/// Work Orders Bounded Context database context
/// </summary>
public class WorkOrdersDbContext(DbContextOptions<WorkOrdersDbContext> options) : DbContext(options)
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

        // Apply Work Orders context configuration
        builder.ApplyWorkOrderConfiguration();

        // Apply snake_case naming convention
        builder.UseSnakeCaseNamingConvention();
    }
}
