using Microsoft.EntityFrameworkCore;
using EntityFrameworkCore.CreatedUpdatedDate.Extensions;
using OsitoPolarPlatform.API.EquipmentManagement.Infrastructure.Persistence.EFC.Configuration.Extensions;
using OsitoPolarPlatform.API.Shared.Infrastructure.Persistence.EFC.Configuration.Extensions;

namespace OsitoPolarPlatform.API.EquipmentManagement.Infrastructure.Persistence.EFC.Configuration;

/// <summary>
/// Equipment Management Bounded Context database context
/// </summary>
public class EquipmentDbContext(DbContextOptions<EquipmentDbContext> options) : DbContext(options)
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

        // Apply Equipment Management context configuration
        builder.ApplyEquipmentConfiguration();

        // Apply snake_case naming convention
        builder.UseSnakeCaseNamingConvention();
    }
}
