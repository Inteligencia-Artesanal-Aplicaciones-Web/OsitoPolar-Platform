using Microsoft.EntityFrameworkCore;
using OsitoPolarPlatform.API.Profiles.Domain.Model.Aggregates;
using OsitoPolarPlatform.API.Profiles.Domain.Repositories;
using OsitoPolarPlatform.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using OsitoPolarPlatform.API.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace OsitoPolarPlatform.API.Profiles.Infrastructure.Persistence.EFC.Repositories;

/// <summary>
/// Owner repository implementation
/// </summary>
public class OwnerRepository(AppDbContext context) : BaseRepository<Owner>(context), IOwnerRepository
{
    public async Task<Owner?> FindByUserIdAsync(int userId)
    {
        return await Context.Set<Owner>().FirstOrDefaultAsync(o => o.UserId == userId);
    }

    public async Task<Owner?> FindByEmailAsync(string email)
    {
        return await Context.Set<Owner>().FirstOrDefaultAsync(o => o.EmailAddress == email);
    }

    public async Task<bool> ExistsByUserIdAsync(int userId)
    {
        return await Context.Set<Owner>().AnyAsync(o => o.UserId == userId);
    }
}
