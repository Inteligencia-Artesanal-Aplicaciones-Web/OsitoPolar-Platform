using OsitoPolarPlatform.API.bc_technicians.Domain.Model.Entities;
using OsitoPolarPlatform.API.bc_technicians.Domain.Model.Queries;
using OsitoPolarPlatform.API.bc_technicians.Domain.Repositories;
using OsitoPolarPlatform.API.bc_technicians.Domain.Services;
using OsitoPolarPlatform.API.WorkOrders.Domain.Repositories; 

namespace OsitoPolarPlatform.API.bc_technicians.Application.Internal.QueryServices;

/// <summary>
/// Concrete implementation of ITechnicianQueryService that handles queries related to technicians,
/// including retrieval by ID, listing all technicians, and calculating average ratings based on customer feedback.
/// </summary>
public class TechnicianQueryService(
    ITechnicianRepository technicianRepository,
    IWorkOrderRepository workOrderRepository) : ITechnicianQueryService 
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
        
        var assignedWorkOrders = await workOrderRepository.ListAsync(); 
        var ratings = assignedWorkOrders
            .Where(wo => wo.AssignedTechnicianId == query.TechnicianId && wo.CustomerFeedbackRating.HasValue)
            .Select(wo => wo.CustomerFeedbackRating!.Value) 
            .ToList();

        return ratings.Any() ? ratings.Average() : 0.0;
    }
}