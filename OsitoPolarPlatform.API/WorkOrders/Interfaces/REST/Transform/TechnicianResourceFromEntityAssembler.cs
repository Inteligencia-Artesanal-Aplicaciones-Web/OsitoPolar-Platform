using OsitoPolarPlatform.API.WorkOrders.Domain.Model.Entities;
using OsitoPolarPlatform.API.WorkOrders.Interfaces.REST.Resources;

namespace OsitoPolarPlatform.API.WorkOrders.Interfaces.REST.Transform;

public static class TechnicianResourceFromEntityAssembler
{
    public static TechnicianResource ToResourceFromEntity(Technician entity, decimal averageRating = 0.0m)
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
