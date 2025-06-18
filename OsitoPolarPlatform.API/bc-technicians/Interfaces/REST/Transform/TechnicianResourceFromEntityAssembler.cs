using OsitoPolarPlatform.API.bc_technicians.Domain.Model.Entities;
using OsitoPolarPlatform.API.bc_technicians.Interfaces.REST.Resources;

namespace OsitoPolarPlatform.API.bc_technicians.Interfaces.REST.Transform;

public static class TechnicianResourceFromEntityAssembler
{
    public static TechnicianResource ToResourceFromEntity(Technician entity)
    {
        return new TechnicianResource
        (
            entity.Id,
            entity.Name,
            entity.Specialization,
            entity.Phone,
            entity.Email,
            entity.Rating,
            entity.Availability,
            entity.CompanyId
        );
    }
}