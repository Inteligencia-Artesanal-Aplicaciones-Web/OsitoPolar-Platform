using OsitoPolarPlatform.API.bc_technicians.Domain.Model.Entities;
using OsitoPolarPlatform.API.bc_technicians.Interfaces.REST.Resources;

namespace OsitoPolarPlatform.API.bc_technicians.Interfaces.REST.Transform;

public static class TechnicianResourceFromEntityAssembler
{
    public static TechnicianResource ToResourceFromEntity(Technician entity, double averageRating = 0.0) 
    {
        return new TechnicianResource
        (
            entity.Id,
            entity.Name,
            entity.Specialization,
            entity.Phone,
            entity.Email,
            averageRating,
            entity.Availability,
            entity.CompanyId
        );
    }
}