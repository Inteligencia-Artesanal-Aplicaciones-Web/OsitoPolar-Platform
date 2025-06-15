using OsitoPolarPlatform.API.ServiceRequests.Domain.Model.ValueObjects;
using OsitoPolarPlatform.API.ServiceRequests.Domain.Repositories;
using OsitoPolarPlatform.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using OsitoPolarPlatform.API.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;
using OsitoPolarPlatform.API.ServiceRequests.Domain.Model.Aggregates;

namespace OsitoPolarPlatform.API.ServiceRequests.Infrastructure.Persistence.EFC.Repositories;


public class ServiceRequestRepository(AppDbContext context) : BaseRepository<ServiceRequest>(context), IServiceRequestRepository
{
    public async Task<IEnumerable<ServiceRequest>> FindByStatusAsync(EServiceRequestStatus status)
    {
        return await Context.Set<ServiceRequest>()
            //.Include(sr => sr.Equipment)
            //.Include(sr => sr.ReportedByUser)
            //.Include(sr => sr.AssignedTechnician)
            .Where(sr => sr.Status == status)
            .ToListAsync();
    }

    public async Task<IEnumerable<ServiceRequest>> FindByEquipmentIdAsync(int equipmentId)
    {
        return await Context.Set<ServiceRequest>()
            //.Include(sr => sr.Equipment)
            //.Include(sr => sr.ReportedByUser)
            //.Include(sr => sr.AssignedTechnician)
            .Where(sr => sr.EquipmentId == equipmentId)
            .ToListAsync();
    }

    public async Task<IEnumerable<ServiceRequest>> FindByAssignedTechnicianIdAsync(int technicianId)
    {
        return await Context.Set<ServiceRequest>()
            //.Include(sr => sr.Equipment)
            //.Include(sr => sr.ReportedByUser)
            //.Include(sr => sr.AssignedTechnician)
            .Where(sr => sr.AssignedTechnicianId == technicianId)
            .ToListAsync();
    }

    public async Task<bool> ExistsByOrderNumberAsync(string orderNumber)
    {
        return await Context.Set<ServiceRequest>()
            .AnyAsync(sr => sr.OrderNumber == orderNumber);
    }

    public new async Task<ServiceRequest?> FindByIdAsync(int id)
    {
        return await Context.Set<ServiceRequest>()
            //.Include(sr => sr.Equipment)
            //.Include(sr => sr.ReportedByUser)
            //.Include(sr => sr.AssignedTechnician)
            .FirstOrDefaultAsync(sr => sr.Id == id);
    }

    public new async Task<IEnumerable<ServiceRequest>> ListAsync()
    {
        return await Context.Set<ServiceRequest>()
            //.Include(sr => sr.Equipment)
            //.Include(sr => sr.ReportedByUser)
            //.Include(sr => sr.AssignedTechnician)
            .ToListAsync();
    }
}
