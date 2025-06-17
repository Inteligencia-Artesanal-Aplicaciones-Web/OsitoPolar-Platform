using OsitoPolarPlatform.API.bc_technicians.Domain.Model.Entities;
using OsitoPolarPlatform.API.bc_technicians.Domain.Model.Queries;
using OsitoPolarPlatform.API.bc_technicians.Domain.Repositories;
using OsitoPolarPlatform.API.bc_technicians.Domain.Services;

namespace OsitoPolarPlatform.API.bc_technicians.Application.Internal.QueryServices;

public class TechnicianQueryService(ITechnicianRepository technicianRepository) : ITechnicianQueryService
{
    public async Task<Technician?> Handle(GetTechnicianByIdQuery query)
    {
        return await technicianRepository.FindByIdAsync(query.TechnicianId);
    }

    public async Task<IEnumerable<Technician>> Handle(GetAllTechniciansQuery query)
    {
        return await technicianRepository.ListAsync();
    }
    
}