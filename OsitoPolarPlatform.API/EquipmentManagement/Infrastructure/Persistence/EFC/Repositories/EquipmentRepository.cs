using OsitoPolarPlatform.API.EquipmentManagement.Domain.Model.Aggregates;
using OsitoPolarPlatform.API.EquipmentManagement.Domain.Repositories;
using OsitoPolarPlatform.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using OsitoPolarPlatform.API.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace OsitoPolarPlatform.API.EquipmentManagement.Infrastructure.Persistence.EFC.Repositories;

public class EquipmentRepository(AppDbContext context) : BaseRepository<Equipment>(context), IEquipmentRepository
{
    public async Task<IEnumerable<Equipment>> FindByOwnerIdAsync(int ownerId)
    {
        return await Context.Set<Equipment>()
            .Where(e => e.OwnerId == ownerId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Equipment>> FindByTypeAsync(string equipmentType)
    {
        return await Context.Set<Equipment>()
            .Where(e => e.Type.ToString().ToLower() == equipmentType.ToLower())
            .ToListAsync();
    }

    public async Task<IEnumerable<Equipment>> FindByStatusAsync(string status)
    {
        return await Context.Set<Equipment>()
            .Where(e => e.Status.ToString().ToLower() == status.ToLower())
            .ToListAsync();
    }

    public async Task<bool> ExistsBySerialNumberAsync(string serialNumber)
    {
        return await Context.Set<Equipment>()
            .AnyAsync(e => e.SerialNumber == serialNumber);
    }

    public async Task<bool> ExistsByCodeAsync(string code)
    {
        return await Context.Set<Equipment>()
            .AnyAsync(e => e.Code == code);
    }

    // ========== RENTAL EQUIPMENT QUERIES ==========

    public async Task<IEnumerable<Equipment>> FindAvailableForRentAsync()
    {
        // Query equipment available in the rental marketplace
        // This is equipment owned by providers that is not yet rented
        return await Context.Set<Equipment>()
            .FromSqlRaw(@"
                SELECT * FROM equipment
                WHERE owner_type = 'Provider'
                AND rental_monthly_fee IS NOT NULL
                AND rental_provider_id IS NOT NULL
                AND rental_start_date IS NULL
                AND rental_end_date IS NULL")
            .ToListAsync();
    }

    public async Task<IEnumerable<Equipment>> FindAvailableForRentByTypeAsync(string type)
    {
        // Query equipment available in the rental marketplace filtered by type
        return await Context.Set<Equipment>()
            .FromSqlRaw(@"
                SELECT * FROM equipment
                WHERE owner_type = 'Provider'
                AND rental_monthly_fee IS NOT NULL
                AND rental_provider_id IS NOT NULL
                AND rental_start_date IS NULL
                AND rental_end_date IS NULL
                AND LOWER(type) = {0}", type.ToLower())
            .ToListAsync();
    }

    public async Task<IEnumerable<Equipment>> FindRentalByProviderIdAsync(int providerId)
    {
        return await Context.Set<Equipment>()
            .Where(e => e.RentalInfo != null &&
                        e.RentalInfo.ProviderId == providerId)
            .ToListAsync();
    }
}