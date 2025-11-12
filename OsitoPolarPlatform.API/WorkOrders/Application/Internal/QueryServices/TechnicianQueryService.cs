using OsitoPolarPlatform.API.WorkOrders.Domain.Model.Entities;
using OsitoPolarPlatform.API.WorkOrders.Domain.Model.Queries;
using OsitoPolarPlatform.API.WorkOrders.Domain.Repositories;
using OsitoPolarPlatform.API.WorkOrders.Domain.Services;
using OsitoPolarPlatform.API.WorkOrders.Interfaces.ACL;

namespace OsitoPolarPlatform.API.WorkOrders.Application.Internal.QueryServices;

/// <summary>
/// Concrete implementation of ITechnicianQueryService that handles queries related to technicians,
/// including retrieval by ID, listing all technicians, and calculating average ratings based on customer feedback.
/// </summary>
public class TechnicianQueryService(
    ITechnicianRepository technicianRepository,
    IWorkOrderContextFacade workOrderFacade) : ITechnicianQueryService
{
    public async Task<Technician?> Handle(GetTechnicianByIdQuery query)
    {
        return await technicianRepository.FindByIdAsync(query.TechnicianId);
    }

    public async Task<IEnumerable<Technician>> Handle(GetAllTechniciansQuery query)
    {
        return await technicianRepository.ListAsync();
    }

    public async Task<double> Handle(GetTechnicianAverageRatingQuery query)
    {
        var technician = await technicianRepository.FindByIdAsync(query.TechnicianId);
        if (technician == null) return 0.0;

        return await workOrderFacade.GetTechnicianAverageRating(query.TechnicianId);
    }
}
