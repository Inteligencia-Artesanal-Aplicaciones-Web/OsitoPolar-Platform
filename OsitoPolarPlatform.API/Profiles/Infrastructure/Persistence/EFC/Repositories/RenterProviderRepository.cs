using Microsoft.EntityFrameworkCore;
using OsitoPolarPlatform.API.Profiles.Domain.Model.Aggregates;
using OsitoPolarPlatform.API.Profiles.Domain.Repositories;
using OsitoPolarPlatform.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using OsitoPolarPlatform.API.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace OsitoPolarPlatform.API.Profiles.Infrastructure.Persistence.EFC.Repositories;

/// <summary>
/// RenterProvider repository implementation
/// </summary>
public class RenterProviderRepository(AppDbContext context) : BaseRepository<RenterProvider>(context), IRenterProviderRepository
{
    public async Task<RenterProvider?> FindByUserIdAsync(int userId)
    {
        return await Context.Set<RenterProvider>().FirstOrDefaultAsync(rp => rp.UserId == userId);
    }

    public async Task<RenterProvider?> FindByEmailAsync(string email)
    {
        return await Context.Set<RenterProvider>().FirstOrDefaultAsync(rp => rp.EmailAddress == email);
    }

    public async Task<RenterProvider?> FindByCompanyNameAsync(string companyName)
    {
        return await Context.Set<RenterProvider>().FirstOrDefaultAsync(rp => rp.CompanyName == companyName);
    }

    public async Task<bool> ExistsByUserIdAsync(int userId)
    {
        return await Context.Set<RenterProvider>().AnyAsync(rp => rp.UserId == userId);
    }
}
