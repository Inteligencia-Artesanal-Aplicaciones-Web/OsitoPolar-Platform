using OsitoPolarPlatform.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using OsitoPolarPlatform.API.Shared.Infrastructure.Persistence.EFC.Repositories; 
using OsitoPolarPlatform.API.WorkOrders.Domain.Model.Aggregates;
using OsitoPolarPlatform.API.WorkOrders.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace OsitoPolarPlatform.API.WorkOrders.Infrastructure.Persistence.EFC.Repositories;

/// <summary>
/// EF Core implementation of IWorkOrderRepository using BaseRepository.
/// </summary>

public class WorkOrderRepository(AppDbContext context) : BaseRepository<WorkOrder>(context), IWorkOrderRepository
{

    public async Task<WorkOrder?> FindByWorkOrderNumberAsync(string workOrderNumber)
    {
        return await Context.Set<WorkOrder>().FirstOrDefaultAsync(wo => wo.WorkOrderNumber == workOrderNumber);
    }

    public async Task<WorkOrder?> FindByServiceRequestIdAsync(int serviceRequestId)
    {
        return await Context.Set<WorkOrder>().FirstOrDefaultAsync(wo => wo.ServiceRequestId == serviceRequestId);
    }
}