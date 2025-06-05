using OsitoPolarPlatform.API.Shared.Domain.Repositories;
using OsitoPolarPlatform.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore;

namespace OsitoPolarPlatform.API.Shared.Infrastructure.Persistence.EFC.Repositories;

public class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class
{
    // ReSharper disable once MemberCanBePrivate.Global
    protected readonly AppDbContext Context;

    protected BaseRepository(AppDbContext context)
    {
        Context = context;
    }

    /// <inheritdoc />
    public async Task AddAsync(TEntity entity)
    {
        await Context.Set<TEntity>().AddAsync(entity);
    }

    /// <inheritdoc />
    public async Task<TEntity?> FindByIdAsync(int id)
    {
        return await Context.Set<TEntity>().FindAsync(id);
    }

    /// <inheritdoc />
    public void Update(TEntity entity)
    {
        Context.Set<TEntity>().Update(entity);
    }

    /// <inheritdoc />
    public void Remove(TEntity entity)
    {
        Context.Set<TEntity>().Remove(entity);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TEntity>> ListAsync()
    {
        return await Context.Set<TEntity>().ToListAsync();
    }
}