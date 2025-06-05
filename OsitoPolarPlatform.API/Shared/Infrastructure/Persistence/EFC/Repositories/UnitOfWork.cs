using OsitoPolarPlatform.API.Shared.Domain.Repositories;
using OsitoPolarPlatform.API.Shared.Infrastructure.Persistence.EFC.Configuration;

namespace OsitoPolarPlatform.API.Shared.Infrastructure.Persistence.EFC.Repositories;

public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    /// <inheritdoc />
    public async Task CompleteAsync()
    {
        await context.SaveChangesAsync();
    }
}