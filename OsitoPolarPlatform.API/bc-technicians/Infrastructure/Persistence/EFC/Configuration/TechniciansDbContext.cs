using Microsoft.EntityFrameworkCore;
using EntityFrameworkCore.CreatedUpdatedDate.Extensions;
using OsitoPolarPlatform.API.bc_technicians.Infrastructure.Persistence.EFC.Configuration.Extensions;
using OsitoPolarPlatform.API.Shared.Infrastructure.Persistence.EFC.Configuration.Extensions;

namespace OsitoPolarPlatform.API.bc_technicians.Infrastructure.Persistence.EFC.Configuration;

/// <summary>
/// Technicians Bounded Context database context
/// </summary>
public class TechniciansDbContext(DbContextOptions<TechniciansDbContext> options) : DbContext(options)
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

        // Apply Technicians context configuration
        builder.ApplyTechnicianConfiguration();

        // Apply snake_case naming convention
        builder.UseSnakeCaseNamingConvention();
    }
}
