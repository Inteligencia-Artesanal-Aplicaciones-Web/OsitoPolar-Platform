using Microsoft.EntityFrameworkCore;
using EntityFrameworkCore.CreatedUpdatedDate.Extensions;
using OsitoPolarPlatform.API.Profiles.Infrastructure.Persistence.EFC.Configuration.Extensions;
using OsitoPolarPlatform.API.Shared.Infrastructure.Persistence.EFC.Configuration.Extensions;

namespace OsitoPolarPlatform.API.Profiles.Infrastructure.Persistence.EFC.Configuration;

/// <summary>
/// Profiles Bounded Context database context
/// </summary>
public class ProfilesDbContext(DbContextOptions<ProfilesDbContext> options) : DbContext(options)
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

        // Apply Profiles context configuration
        builder.ApplyProfilesConfiguration();

        // Apply snake_case naming convention
        builder.UseSnakeCaseNamingConvention();
    }
}
